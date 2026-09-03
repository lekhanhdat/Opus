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
using System.Collections;

namespace AvePoint.ObjectModel.Server19.Office
{
    class AveOCategory : IAveOCategory
    {
        private Category mCategory;

        public AveOCategory(Category category)
        {
            mCategory = category;
        }

        #region IAveOCategory Members

        public IEnumerable GetUnmappedCrawledProperties()
        {
            List<AveOCrawledProperty> crawledProperties = new List<AveOCrawledProperty>();
            foreach (CrawledProperty crawledProperty in mCategory.GetUnmappedCrawledProperties())
            {
                if (crawledProperty != null)
                {
                    crawledProperties.Add(new AveOCrawledProperty(crawledProperty));
                }
                else
                {
                    crawledProperties.Add(null);
                }
            }
            return crawledProperties;
        }

        public bool Contains(Guid propset)
        {
            return mCategory.Contains(propset);
        }

        public IEnumerable GetAllCrawledProperties()
        {
            List<AveOCrawledProperty> crawledProperties = new List<AveOCrawledProperty>();
            foreach (CrawledProperty crawledProperty in mCategory.GetAllCrawledProperties())
            {
                if (crawledProperty != null)
                {
                    crawledProperties.Add(new AveOCrawledProperty(crawledProperty));
                }
                else
                {
                    crawledProperties.Add(null);
                }
            }
            return crawledProperties;
        }

        public bool AutoCreateNewManagedProperties
        {
            get
            {
                return mCategory.AutoCreateNewManagedProperties;
            }

            set
            {
                mCategory.AutoCreateNewManagedProperties = value;
            }
        }

        public int CrawledPropertyCount
        {
            get
            {
                return mCategory.CrawledPropertyCount;
            }
        }

        public bool DiscoverNewProperties
        {
            get
            {
                return mCategory.DiscoverNewProperties;
            }
            set
            {
                mCategory.DiscoverNewProperties = value;
            }
        }

        public bool FullTextQueriable
        {
            get
            {
                return mCategory.FullTextQueriable;
            }
            set
            {
                mCategory.FullTextQueriable = value;
            }
        }

        public bool MapToContents
        {
            get
            {
                return mCategory.MapToContents;
            }
            set
            {
                mCategory.MapToContents = value;
            }
        }

        public bool MatchExistingManagedProperty
        {
            get
            {
                return mCategory.MatchExistingManagedProperty;
            }
            set
            {
                mCategory.MatchExistingManagedProperty = value;
            }
        }

        public string MatchIgnorePrefix
        {
            get
            {
                return mCategory.MatchIgnorePrefix;
            }
            set
            {
                mCategory.MatchIgnorePrefix = value;
            }
        }

        public string MatchIgnoreSuffix
        {
            get
            {
                return mCategory.MatchIgnoreSuffix;
            }
            set
            {
                mCategory.MatchIgnoreSuffix = value;
            }
        }

        public int MaxIndexedStringLength
        {
            get
            {
                return mCategory.MaxIndexedStringLength;
            }
            set
            {
                mCategory.MaxIndexedStringLength = value;
            }
        }

        public int MaxNonIndexedStringLength
        {
            get
            {
                return mCategory.MaxNonIndexedStringLength;
            }
            set
            {
                mCategory.MaxNonIndexedStringLength = value;
            }
        }

        public bool MultipleValues
        {
            get
            {
                return mCategory.MultipleValues;
            }
            set
            {
                mCategory.MultipleValues = value;
            }
        }

        public string Name
        {
            get
            {
                return mCategory.Name;
            }
            set
            {
                mCategory.Name = value;
            }
        }

        public bool Queryable
        {
            get
            {
                return mCategory.Queryable;
            }
            set
            {
                mCategory.Queryable = value;
            }
        }

        public bool Retrievable
        {
            get
            {
                return mCategory.Retrievable;
            }
            set
            {
                mCategory.Retrievable = value;
            }
        }

        public bool Scoped
        {
            get
            {
                return mCategory.Scoped;
            }
            set
            {
                mCategory.Scoped = value;
            }
        }

        public void Update()
        {
            mCategory.Update();
        }

        #endregion
    }
}
