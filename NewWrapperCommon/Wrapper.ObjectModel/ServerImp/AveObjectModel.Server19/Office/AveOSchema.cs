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
using Microsoft.Office.Server.Search.Administration;
using AvePoint.Wrapper.Common.Office;

namespace AvePoint.ObjectModel.Server19.Office
{
    class AveOSchema : IAveOSchema
    {
        private Schema mSchema;
        private AveOCategoryCollection mAllCategories;

        public AveOSchema(Schema schema)
        {
            mSchema = schema;
        }

        public AveOSchema(IAveOSearchServiceApplication aveOSearchServiceApplication)
            : this(new Schema((aveOSearchServiceApplication as AveOSearchServiceApplication).SearchServiceApplication))
        { }

        public AveOSchema(IAveOSearchContext aveOSearchContext)
            : this(new Schema((aveOSearchContext as AveOSearchContext).SearchContext))
        { }

        public IAveOManagedPropertyCollection AllManagedProperties
        {
            get
            {
                if (mSchema.AllManagedProperties != null)
                {
                    return new AveOManagedPropertyCollection(mSchema.AllManagedProperties);
                }
                return null;
            }
        }

        public IAveOCrawledProperty GetCrawledProperty(Guid propset, string name, int variantType)
        {
            CrawledProperty crawledProperty = mSchema.GetCrawledProperty(propset, name, variantType);
            if (crawledProperty == null)
            {
                return null;
            }
            return new AveOCrawledProperty(crawledProperty);
        }

        public IAveOCategoryCollection AllCategories
        {
            get 
            {
                if (mAllCategories == null)
                {
                    CategoryCollection allCategories = mSchema.AllCategories;
                    if (allCategories != null)
                    {
                        mAllCategories = new AveOCategoryCollection(allCategories);
                    }
                }
                return mAllCategories;
            }
        }
    }
}
