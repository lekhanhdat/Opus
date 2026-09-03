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
using System;

namespace AvePoint.ObjectModel.Server19
{
    class AveBestBetCollection : AveAbstractCommonCollection<IAveBestBet>, IAveBestBetCollection
    {
        private BestBetCollection mBestBetCollection;

        public AveBestBetCollection(BestBetCollection bestBetCollection)
            : base(bestBetCollection)
        {
            mBestBetCollection = bestBetCollection;
        }

        protected override object CreatElementInstance(object t)
        {
            return new AveBestBet((BestBet)t);
        }

        public override int Count
        {
            get
            {
                return mBestBetCollection.Count;
            }
        }

        #region IAveBestBetCollection Members

        public bool ContainsBestBet(string name)
        {
            return mBestBetCollection.ContainsBestBet(name);
        }

        public bool RemoveBestBet(string name)
        {
            return mBestBetCollection.RemoveBestBet(name);
        }

        public IAveBestBet GetBestBet(string name)
        {
            BestBet bestBet = mBestBetCollection.GetBestBet(name);
            if (bestBet != null)
            {
                return new AveBestBet(bestBet);
            }
            return null;
        }

        #endregion

    }
}
