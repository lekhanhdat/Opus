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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.RADataBroker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RMExplorer
{
    public sealed class RegistedSiteCache
    {
        private AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(RegistedSiteCache));
        private static RegistedSiteCache _RegistedSiteCache = new RegistedSiteCache();
        private List<RemoteSiteCollection> accountCache = null;

        private readonly object mlock = new object();
        static RegistedSiteCache() { }

        public static RegistedSiteCache CreateInstance()
        {
            return _RegistedSiteCache;
        }

        public RemoteSiteCollection GetAccountInfoBySiteUrl(string siteUrl)
        {
            lock (mlock)
            {
                if (accountCache == null)
                {
                    try
                    {
                        //get account cache from OData
                        //var DAOAPIClientV1 = new DAOAPIClientV1();
                        //accountCache = DAOAPIClientV1.GetAuthorisedRemoteSiteCollectionsByUser();
                        accountCache = RABrowserClient.GetAuthorisedRemoteSiteCollectionsByUser();
                    }
                    catch(Exception ex)
                    {
                        logger.Warn(string.Format("Get authorised remote site collections error, reason : {0}.", ex.ToString()));
                    }
                }
                var siteInfo = accountCache.FirstOrDefault(a => a.url.Equals(siteUrl, StringComparison.OrdinalIgnoreCase));
                if (siteInfo !=null)
                {
                    return siteInfo;
                }
                else
                {
                    //Re-get the account cache ?
                }
                return null;
            }
            
        }
    }
}
