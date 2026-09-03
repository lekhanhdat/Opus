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

namespace AvePoint.ObjectModel.Server13.Office
{
    class AveOCrawlTopologySettings : IAveOCrawlTopologySettings
    {
        private CrawlTopologySettings mCrawlTopologySettings;

        public AveOCrawlTopologySettings(CrawlTopologySettings crawlTopologySettings)
        {
            mCrawlTopologySettings = crawlTopologySettings;
        }

        #region IAveOCrawlTopologySettings members

        public List<IAveOCrawlComponentSettings> CrawlComponents
        {
            get
            {
                List<IAveOCrawlComponentSettings> crawlComponents = null;
                List<CrawlComponentSettings> spCrawlComponents = mCrawlTopologySettings.CrawlComponents;
                if (spCrawlComponents != null)
                {
                    crawlComponents = new List<IAveOCrawlComponentSettings>();
                    foreach (CrawlComponentSettings crawlComponentSettings in spCrawlComponents)
                    {
                        if (crawlComponentSettings != null)
                        {
                            crawlComponents.Add(new AveOCrawlComponentSettings(crawlComponentSettings));
                        }
                        else
                        {
                            crawlComponents.Add(null);
                        }
                    }
                }
                return crawlComponents;
            }
        }

        public List<IAveOCrawlStoreSettings> CrawlStores
        {
            get
            {
                List<IAveOCrawlStoreSettings> crawlStores = null;
                List<CrawlStoreSettings> SPCrawlComponents = mCrawlTopologySettings.CrawlStores;
                if (SPCrawlComponents != null)
                {
                    crawlStores = new List<IAveOCrawlStoreSettings>();
                    foreach (CrawlStoreSettings crawlStore in SPCrawlComponents)
                    {
                        if (crawlStore != null)
                        {
                            crawlStores.Add(new AveOCrawlStoreSettings(crawlStore));
                        }
                        else
                        {
                            crawlStores.Add(null);
                        }
                    }
                }
                return crawlStores;
            }
        }

        #region add for SP2013
        public List<IAveOLinksStoreSettings> LinksStores
        {
            get
            {
                List<IAveOLinksStoreSettings> linksStores = null;
                List<LinksStoreSettings> SPCrawlComponents = mCrawlTopologySettings.LinksStores;
                if (SPCrawlComponents != null)
                {
                    linksStores = new List<IAveOLinksStoreSettings>();
                    foreach (LinksStoreSettings linksStore in SPCrawlComponents)
                    {
                        if (linksStore != null)
                        {
                            linksStores.Add(new AveOLinksStoreSettings(linksStore));
                        }
                        else
                        {
                            linksStores.Add(null);
                        }
                    }
                }
                return linksStores;
            }
        }
        #endregion

        #endregion
    }
}
