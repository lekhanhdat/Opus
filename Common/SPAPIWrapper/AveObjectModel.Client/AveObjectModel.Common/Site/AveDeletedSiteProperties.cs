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
namespace AvePoint.ObjectModel.Common
{
    using System;
    using System.Collections.Generic;
    using Wrapper.Common;

    class AveDeletedSiteProperties : AveClientObject, IAveDeletedSiteProperties
    {
        private IAveRequest mRequest;
        private string mSiteUrl;
        public AveDeletedSiteProperties(IAveRequest request, string siteUrl, Dictionary<string, object> prop)
        {
            mRequest = request;
            mSiteUrl = siteUrl;
            base.DataCache.AddPropertyies(prop);
        }

        public string SiteUrl
        {
            get
            {
                return mSiteUrl;
            }
        }

        public int DaysRemaining
        {
            get
            {
                return base.DataCache.GetProperty<int>("DaysRemaining");
            }
        }

        public DateTime DeletionTime
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("DeletionTime");
            }
        }

        public Guid SiteId
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("SiteId");
            }
        }

        public string Status
        {
            get
            {
                return base.DataCache.GetProperty<string>("Status");
            }
        }

        public long StorageMaximumLevel
        {
            get
            {
                return base.DataCache.GetProperty<long>("StorageMaximumLevel");
            }
        }

        public string Url
        {
            get
            {
                return base.DataCache.GetProperty<string>("Url");
            }
        }

        public double UserCodeMaximumLevel
        {
            get
            {
                return base.DataCache.GetProperty<double>("UserCodeMaximumLevel");
            }
        }
    }
}
