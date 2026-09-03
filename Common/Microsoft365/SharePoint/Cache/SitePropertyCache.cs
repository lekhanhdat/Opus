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
using AvePoint.ObjectModel.ClientOM;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.CompliancePolicy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.ObjectModel.ClientOM
{
    public class SitePropertyCache
    {

        private volatile  IList<ComplianceTag> avaliableComplianceTags;
        public  IList<ComplianceTag> AvaliableComplianceTags
        {
            get => avaliableComplianceTags;
        }

        private SitePropertyCache() { }

        private static SitePropertyCache instance;
        
        public static SitePropertyCache GetInstance()
        {
            if (instance == null)
            {
                lock (typeof(SitePropertyCache))
                {
                    if(instance == null)
                    {
                        instance = new SitePropertyCache();
                    }
                }
            }
            return instance;
        }

        public void InitAvaliableComplianceTags(string siteUrl, ClientContext context)
        {
            if (AvaliableComplianceTags == null)
            {
                lock (typeof(SitePropertyCache))
                {
                    if (AvaliableComplianceTags == null)
                    {
                        avaliableComplianceTags = SPPolicyStoreProxy.GetAvailableTagsForSite(context, siteUrl);
                        context.ExecuteQuery();
                    }
                }
            }
        }
    }
}
