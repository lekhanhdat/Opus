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



using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Search.Extended.Administration.Keywords;
using AvePoint.Wrapper.Common.Search;
using AvePoint.ObjectModel.Server16.Search;
using System;

namespace AvePoint.ObjectModel.Server16
{
    class AvePromotion : AveSearchSetting, IAvePromotion
    {
        private Promotion mPromotion;
        private AvePromotedItemCollection mPromotedItems;

        public AvePromotion(Promotion promotion)
            : base(promotion)
        {
            mPromotion = promotion;
        }

        public int CompareTo(IAvePromotion other)
        {
            if (other == null || !(other is IAvePromotion))
            {
                return 1;
            }
            return mPromotion.CompareTo((other as AvePromotion).mPromotion);
        }

        public int BoostValue
        {
            get
            {
                return mPromotion.BoostValue;
            }
            set
            {
                mPromotion.BoostValue = value;
            }
        }

        public IAvePromotedItemCollection PromotedItems
        {
            get
            {
                if (mPromotedItems == null)
                {
                    PromotedItemCollection promotedItems = mPromotion.PromotedItems;
                    if (promotedItems != null)
                    {
                        mPromotedItems = new AvePromotedItemCollection(promotedItems);
                    }
                }
                return mPromotedItems;
            }
        }
    }
}
