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
using Microsoft.Office.Server.Search.Query;
using System.Collections.Specialized;

namespace AvePoint.ObjectModel.Server19.Office
{
    class AveOLocation : IAveOLocation
    {
        private Location mLocation;

        public AveOLocation(Location location)
        {
            mLocation = location;
        }

        public AveOLocation(string name, IAveOSearchServiceApplicationProxy searchProxy)
            : this(new Location(name, (searchProxy as AveOSearchServiceApplicationProxy).SearchServiceApplicationProxy))
        { }

        internal Location Location
        {
            get
            {
                return mLocation;
            }
        }

        public int StartItem
        {
            get
            {
                return mLocation.StartItem;
            }
            set
            {
                mLocation.StartItem = value;
            }
        }

        public int ItemsPerPage
        {
            get
            {
                return mLocation.ItemsPerPage;
            }
            set
            {
                mLocation.ItemsPerPage = value;
            }
        }

        public string UserQuery
        {
            get
            {
                return mLocation.UserQuery;
            }
            set
            {
                mLocation.UserQuery = value;
            }
        }

        public StringCollection RequestedProperties
        {
            get
            {
                return mLocation.RequestedProperties;
            }
            set
            {
                mLocation.RequestedProperties = value;
            }
        }

        public bool EnableStemming
        {
            get
            {
                return mLocation.EnableStemming;
            }
            set
            {
                mLocation.EnableStemming = value;
            }
        }

        public bool TrimDuplicates
        {
            get
            {
                return mLocation.TrimDuplicates;
            }
            set
            {
                mLocation.TrimDuplicates = value;
            }
        }

        public int TotalResults
        {
            get
            {
                return mLocation.TotalResults;
            }
            set
            {
                mLocation.TotalResults = value;
            }
        }
    }
}
