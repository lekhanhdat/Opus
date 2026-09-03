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

namespace AvePoint.ObjectModel.Server19
{
    abstract class AveCrawledProperty : IAveCrawledProperty
    {
        private CrawledProperty mCrawledProperty;

        public AveCrawledProperty(CrawledProperty crawledProperty)
        {
            mCrawledProperty = crawledProperty;
        }

        public abstract string CategoryName
        {
            get;
        }

        public abstract bool IsMappedToContents
        {
            get;
            set;
        }

        public abstract bool IsMultiValued
        {
            get;
        }

        public abstract bool IsNameEnum
        {
            get;
        }

        public abstract string Name
        {
            get;
        }

        public abstract Guid Propset
        {
            get;
        }

        public abstract int VariantType
        {
            get;
        }

        public abstract void Update();

        public abstract IEnumerable<IAveManagedProperty> GetMappedManagedProperties();
    }
}
