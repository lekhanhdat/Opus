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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint;

namespace AvePoint.ObjectModel.Server19
{
    class AveFeaturePropertyCollection : AveAbstractCommonCollection<IAveFeatureProperty>, IAveFeaturePropertyCollection
    {
        SPFeaturePropertyCollection featurePropertyCollection;
              

        public AveFeaturePropertyCollection(SPFeaturePropertyCollection featurePropertys) : base(featurePropertys)
        {
            this.featurePropertyCollection = featurePropertys;
        }

        public override int Count
        {
            get { return this.featurePropertyCollection.Count; }
        }

        public IAveFeatureProperty this[string propertyName]
        {
            get
            {
                SPFeatureProperty property=this.featurePropertyCollection[propertyName];
                if (property != null)
                {
                    return new AveFeatureProperty(property);
                }
                return null;
            }
        }

        protected override object CreatElementInstance(object t)
        {
            return new AveFeatureProperty(t as SPFeatureProperty);
        }

        public void Update()
        {
            featurePropertyCollection.Update();
        }

        public void Add(IAveFeatureProperty property)
        {
            SPFeatureProperty spProperty = property == null ? null : ((AveFeatureProperty)property).FeatureProperty;
            featurePropertyCollection.Add(spProperty);
        }
    }
}
