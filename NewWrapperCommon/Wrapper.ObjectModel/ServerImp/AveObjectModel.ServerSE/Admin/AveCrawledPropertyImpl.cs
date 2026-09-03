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
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Search.Extended.Administration.Schema;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveCrawledPropertyImpl : AveCrawledProperty, IAveCrawledPropertyImpl
    {
        private object mCrawledPropertyImpl;

        public AveCrawledPropertyImpl(object crawledPropertyImpl)
            : base((CrawledProperty)crawledPropertyImpl)
        {
            mCrawledPropertyImpl = crawledPropertyImpl;
        }

        public override string CategoryName
        {
            get
            {
                return (mCrawledPropertyImpl as CrawledProperty).CategoryName;
            }
        }

        public override bool IsMappedToContents
        {
            get
            {
                return (mCrawledPropertyImpl as CrawledProperty).IsMappedToContents;
            }
            set
            {
                (mCrawledPropertyImpl as CrawledProperty).IsMappedToContents = value;
            }
        }

        public override bool IsMultiValued
        {
            get
            {
                return (mCrawledPropertyImpl as CrawledProperty).IsMultiValued;
            }
        }

        public override bool IsNameEnum
        {
            get
            {
                return (mCrawledPropertyImpl as CrawledProperty).IsNameEnum;
            }
        }

        public override string Name
        {
            get
            {
                return (mCrawledPropertyImpl as CrawledProperty).Name;
            }
        }

        public override Guid Propset
        {
            get
            {
                return (mCrawledPropertyImpl as CrawledProperty).Propset;
            }
        }

        public override int VariantType
        {
            get
            {
                return (mCrawledPropertyImpl as CrawledProperty).VariantType;
            }
        }

        public override void Update()
        {
            (mCrawledPropertyImpl as CrawledProperty).Update();
        }

        public override IEnumerable<IAveManagedProperty> GetMappedManagedProperties()
        {
            IEnumerable<ManagedProperty> managedPropertys = (mCrawledPropertyImpl as CrawledProperty).GetMappedManagedProperties();
            List<IAveManagedProperty> list = new List<IAveManagedProperty>();
            foreach (ManagedProperty managedProperty in managedPropertys)
            {
                if (managedProperty != null)
                {
                    list.Add(new AveManagedProperty(managedProperty));
                }
                else
                {
                    list.Add(null);
                }
            }
            return list;
        }
    }
}
