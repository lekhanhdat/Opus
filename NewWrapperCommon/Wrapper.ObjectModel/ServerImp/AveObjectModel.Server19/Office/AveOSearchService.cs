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



using Microsoft.Office.Server.Search.Administration;
using AvePoint.Wrapper.Common;
using System.Net;
using AvePoint.Wrapper.Common.Office;

namespace AvePoint.ObjectModel.Server19.Office
{
    class AveOSearchService : AveWindowsService, IAveOSearchService
    {
        private SearchService mSearchService;
        private AveServiceApplicationCollection mSearchApplications;
        private AveOSearchService mService;
        private AveOSiteHitRulesCollection mSiteHitRules;

        public AveOSearchService(SearchService searchService)
            : base(searchService)
        {
            mSearchService = searchService;
        }

        public AveOSearchService()
            : this(new SearchService())
        { }

        public IAveOSearchService Service
        {
            get
            {
                if (mService == null)
                {
                    SearchService searchService = SearchService.Service;
                    if (searchService != null)
                    {
                        mService = new AveOSearchService(searchService);
                    }
                }
                return mService;
            }
        }

        #region IAveOSearchService Members

        public int AcknowledgementTimeout
        {
            get
            {
                return mSearchService.AcknowledgementTimeout;
            }
            set
            {
                mSearchService.AcknowledgementTimeout = value;
            }
        }

        public int ConnectionTimeout
        {
            get
            {
                return mSearchService.ConnectionTimeout;
            }
            set
            {
                mSearchService.ConnectionTimeout = value;
            }
        }

        public bool IgnoreSSLWarnings
        {
            get
            {
                return mSearchService.IgnoreSSLWarnings;
            }
            set
            {
                mSearchService.IgnoreSSLWarnings = value;
            }
        }

        public IAveServiceApplicationCollection SearchApplications
        {
            get
            {
                if (mSearchApplications == null)
                {
                    mSearchApplications = new AveServiceApplicationCollection(mSearchService.SearchApplications);
                }
                return mSearchApplications;
            }
        }

        public WebProxy WebProxy
        {
            get { return mSearchService.WebProxy; }
        }

        public IAveOSiteHitRulesCollection SiteHitRules
        {
            get
            {
                if (mSiteHitRules == null)
                {
                    mSiteHitRules = new AveOSiteHitRulesCollection(mSearchService.SiteHitRules);
                }
                return mSiteHitRules;
            }
        }

       

        #endregion


        public string ContactEmail
        {
            get
            {
                return mSearchService.ContactEmail;
            }
            set
            {
                mSearchService.ContactEmail = value;
            }
        }
    }
}
