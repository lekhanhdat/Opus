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
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Upgrade;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveSiteCollectionCopier : IAveSiteCollectionCopier
    {
        private const string mSiteCollectionCopier_Type = "Microsoft.SharePoint.Upgrade.SPSiteCollectionCopier";
        

        IAveContentDatabase mDBFrom;
        IAveContentDatabase mDBTo;
        List<IAveSite> mColSites;

        

        public AveSiteCollectionCopier(IAveContentDatabase dbFrom, IAveContentDatabase dbTo, List<IAveSite> colSites)           
        {
            mDBFrom = dbFrom;
            mDBTo = dbTo;
            mColSites = colSites;
        }

        public void Move(AveSiteLockModifier sourceSiteLockType, Dictionary<int, int> rbsProviderMap, out Dictionary<IAveSite, string> failedSites)
        {
            List<SPSite> sites = new List<SPSite>();
            foreach (AveSite site in mColSites)
            {
                sites.Add(site.Site);
            }
            List<SPDeletedSite> deletedSites = new List<SPDeletedSite>();
            object siteCollectionCopier = AveAssemblyUtility.CreateInstance(mSiteCollectionCopier_Type, new Type[] { typeof(SPContentDatabase), typeof(SPContentDatabase), typeof(List<SPSite>), typeof(List<SPDeletedSite>) }, new object[] { ((AveContentDatabase)mDBFrom).ContentDatabase, ((AveContentDatabase)mDBTo).ContentDatabase, sites, deletedSites });
            Type[] paramTypes = new Type[] { AveAssemblyUtility.GetType("Microsoft.SharePoint.Upgrade.SiteLockModifier"), typeof(Dictionary<int, int>), typeof(Dictionary<SPSite, string>).MakeByRefType(), typeof(Dictionary<SPDeletedSite, string>) };
            object[] parameters = new object[4];
            parameters[0] = Enum.Parse(AveAssemblyUtility.GetType("Microsoft.SharePoint.Upgrade.SiteLockModifier"), sourceSiteLockType.ToString());
            parameters[1] = rbsProviderMap;

            AveAssemblyUtility.InvokeGenericMethod(siteCollectionCopier, "Move", parameters, paramTypes);

            Dictionary<SPSite, string> dictionary = parameters[2] as Dictionary<SPSite, string>;            
            if (null == dictionary)
            {
                failedSites = null;
            }
            else
            {
                failedSites = new Dictionary<IAveSite, string>();
                foreach (KeyValuePair<SPSite, string> oneFailSite in dictionary)
                {
                    failedSites.Add(new AveSite(oneFailSite.Key), oneFailSite.Value);
                }
            }
        }

        public void Move(AveSiteLockModifier sourceSiteLockType, out Dictionary<IAveSite, string> failedSites)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            //mDBFrom = null;
            //mDBTo = null;
            //mColSites = null;
        }
    }
}
