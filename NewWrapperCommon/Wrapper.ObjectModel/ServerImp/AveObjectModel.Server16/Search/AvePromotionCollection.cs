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
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Search.Extended.Administration.Keywords;
using System.Collections;

namespace AvePoint.ObjectModel.Server16
{
    class AvePromotionCollection : AveAbstractCommonCollection<IAvePromotion>, IAvePromotionCollection
    {
        private PromotionCollection mPromotionCollection;

        public AvePromotionCollection(PromotionCollection promotionCollection)
            : base(promotionCollection)
        {
            mPromotionCollection = promotionCollection;
        }

        protected override object CreatElementInstance(object t)
        {
            return new AvePromotion((Promotion)t);
        }

        public override int Count
        {
            get
            {
                return mPromotionCollection.Count;
            }
        }

        public int DemotionCount
        {
            get
            {
                return mPromotionCollection.DemotionCount;
            }
        }

        public IAvePromotion this[string identity]
        {
            get
            {
                Promotion promotion = mPromotionCollection[identity];
                if (promotion != null)
                {
                    return new AvePromotion(promotion);
                }
                return null;
            }
        }

        public int PromotionCount
        {
            get
            {
                return mPromotionCollection.PromotionCount;
            }
        }

        public IAvePromotion AddPromotion(string name)
        {
            Promotion promotion = mPromotionCollection.AddPromotion(name);
            if (promotion == null)
            {
                return null;
            }
            return new AvePromotion(promotion);
        }

        public IAvePromotion AddPromotion(string name, Wrapper.Common.Search.IAveContext context)
        {
            Promotion promotion = mPromotionCollection.AddPromotion(name, (context as Search.AveContext).Context);
            if (promotion == null)
            {
                return null;
            }
            return new AvePromotion(promotion);
        }

        public IAvePromotion AddPromotion(string name, string description, int boostValue, DateTime? start, DateTime? end, Wrapper.Common.Search.IAveContext context)
        {
            Promotion promotion = mPromotionCollection.AddPromotion(name, description, boostValue, start, end, (context as Search.AveContext).Context);
            if (promotion == null)
            {
                return null;
            }
            return new AvePromotion(promotion);
        }

        public bool ContainsPromotion(string name)
        {
            return mPromotionCollection.ContainsPromotion(name);
        }

        public IEnumerator<IAvePromotion> GetDemotionEnumerator()
        {
            return base.GetEnumerator();
        }

        public IAvePromotion GetPromotion(string name)
        {
            Promotion promotion = mPromotionCollection.GetPromotion(name);
            if (promotion != null)
            {
                return new AvePromotion(promotion);
            }
            return null;
        }

        public IEnumerator<IAvePromotion> GetPromotionEnumerator()
        {
            return base.GetEnumerator();
        }

        public bool RemovePromotion(string name)
        {
            return mPromotionCollection.RemovePromotion(name);
        }
    }
}
