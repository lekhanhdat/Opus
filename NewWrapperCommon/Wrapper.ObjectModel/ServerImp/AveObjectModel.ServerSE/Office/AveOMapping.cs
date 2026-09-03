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
using Microsoft.Office.Server.Search.Administration;

namespace AvePoint.ObjectModel.ServerSE.Office
{
    class AveOMapping : IAveOMapping
    {
        private Mapping mMapping;

        public AveOMapping(Mapping mapping)
        {
            mMapping = mapping;
        }

        public AveOMapping()
            : this(new Mapping())
        { }

        public AveOMapping(Guid crawledPropset, string crawledPropertyName, int crawledPropertyVariantType, int managedPid)
            :this(new Mapping(crawledPropset, crawledPropertyName, crawledPropertyVariantType, managedPid))
        { }

        internal Mapping Mapping
        {
            get
            {
                return mMapping;
            }
        }

        public Guid CrawledPropset
        {
            get
            {
                return mMapping.CrawledPropset;
            }
            set
            {
                mMapping.CrawledPropset = value;
            }
        }

        public string CrawledPropertyName
        {
            get
            {
                return mMapping.CrawledPropertyName;
            }
            set
            {
                mMapping.CrawledPropertyName = value;
            }
        }

        public int CrawledPropertyVariantType
        {
            get
            {
                return mMapping.CrawledPropertyVariantType;
            }
            set
            {
                mMapping.CrawledPropertyVariantType = value;
            }
        }

        public int CompareTo(IAveOMapping other)
        {
            return mMapping.CompareTo((other as AveOMapping).Mapping);
        }

        public bool Equals(IAveOMapping other)
        {
            return mMapping.Equals((other as AveOMapping).Mapping);
        }

        public int ManagedPid
        {
            get
            {
                return mMapping.ManagedPid;
            }
            set
            {
                mMapping.ManagedPid = value;
            }
        }
    }
}
