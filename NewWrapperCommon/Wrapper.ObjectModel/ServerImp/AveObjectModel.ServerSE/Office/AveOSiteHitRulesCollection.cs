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
using AvePoint.Wrapper.Common.Office;
using Microsoft.Office.Server.Search.Administration;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.ServerSE.Office
{
    class AveOSiteHitRulesCollection : AveAbstractCommonCollection<IAveOSiteHitRule>, IAveOSiteHitRulesCollection
    {
        private SiteHitRulesCollection mSiteHitRulesCollection;

        public AveOSiteHitRulesCollection(SiteHitRulesCollection siteHitRulesCollection)
            : base(siteHitRulesCollection)
        {
            mSiteHitRulesCollection = siteHitRulesCollection;
        }

        public IAveOSiteHitRule Create(string site, int hitRate, AveSiteHitRuleBehavior behavior)
        {
            return new AveOSiteHitRule(mSiteHitRulesCollection.Create(site, hitRate, (SiteHitRuleBehavior)behavior));
        }

        public bool Exists(string site)
        {
            return mSiteHitRulesCollection.Exists(site);
        }

        public void LowerPriority(IAveOSiteHitRule rule)
        {
            mSiteHitRulesCollection.LowerPriority((rule as AveOSiteHitRule).SiteHitRule);
        }

        public void RaisePriority(IAveOSiteHitRule rule)
        {
            mSiteHitRulesCollection.RaisePriority((rule as AveOSiteHitRule).SiteHitRule);
        }

        public void Remove(IAveOSiteHitRule rule)
        {
            AveAssemblyUtility.InvokeMethod(mSiteHitRulesCollection, "Remove", new Type[] { typeof(SiteHitRule) }, new object[] { (rule as AveOSiteHitRule).SiteHitRule });
        }

        public IAveOSiteHitRule this[string site]
        {
            get
            {
                SiteHitRule siteHitRule = mSiteHitRulesCollection[site];
                if (siteHitRule != null)
                {
                    return new AveOSiteHitRule(siteHitRule);
                }
                return null;
            }
        }

        protected override object CreatElementInstance(object t)
        {
            return new AveOSiteHitRule(t as SiteHitRule);
        }

        public override int Count
        {
            get
            {
                return mSiteHitRulesCollection.Count;
            }
        }

        public override IAveOSiteHitRule this[int index]
        {
            get
            {
                SiteHitRule siteHitRule = mSiteHitRulesCollection[index];
                if (siteHitRule != null)
                {
                    return new AveOSiteHitRule(siteHitRule);
                }
                return null;
            }
        }
    }
}
