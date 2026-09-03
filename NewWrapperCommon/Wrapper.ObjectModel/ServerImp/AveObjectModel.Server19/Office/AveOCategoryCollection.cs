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
    class AveOCategoryCollection : AveAbstractCommonCollection, IAveOCategoryCollection
    {
        private CategoryCollection mCategoryCollection;

        public AveOCategoryCollection(CategoryCollection categoryCollection)
            : base(categoryCollection)
        {
            mCategoryCollection = categoryCollection;
        }

        internal override object CreatElementInstance(object obj)
        {
            return new AveOCategory((Category)obj);
        }

        public IAveOCategory this[string name]
        {
            get 
            {
                if (mCategoryCollection[name] != null)
                {
                    return new AveOCategory(mCategoryCollection[name]);
                }
                return null;
            }
        }

        public IAveOCategory this[Guid propset]
        {
            get
            {
                if (mCategoryCollection[propset] != null)
                {
                    return new AveOCategory(mCategoryCollection[propset]);
                }
                return null;
            }
        }

        public bool Contains(Guid propset)
        {
            return mCategoryCollection.Contains(propset);
        }

        public bool Contains(string name)
        {
            return mCategoryCollection.Contains(name);
        }

        public IAveOCategory Create(string name, Guid propset)
        {
            return new AveOCategory(mCategoryCollection.Create(name, propset));
        }
    }
}
