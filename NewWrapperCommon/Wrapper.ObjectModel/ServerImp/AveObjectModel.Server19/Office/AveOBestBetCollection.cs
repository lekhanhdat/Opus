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
using AvePoint.Wrapper.Common.Office;
using Microsoft.Office.Server.Search.Administration;
using System.Collections;

namespace AvePoint.ObjectModel.Server19.Office
{
    class AveOBestBetCollection : AveAbstractCommonCollection, IAveOBestBetCollection
    {
        private BestBetCollection mBestBetCollection;

        public AveOBestBetCollection(BestBetCollection bestBetCollection)
            : base(bestBetCollection)
        {
            mBestBetCollection = bestBetCollection;
        }

        internal BestBetCollection BestBetCollection
        {
            get
            {
                return mBestBetCollection;
            }
        }

        public int Count
        {
            get
            {
                return mBestBetCollection.Count;
            }
        }

        public IAveOBestBet Create(string title, string description, Uri url)
        {
            BestBet bestBet = mBestBetCollection.Create(title, description, url);
            if (bestBet != null)
            {
                return new AveOBestBet(bestBet);
            }
            return null;
        }

        internal override object CreatElementInstance(object obj)
        {
            return new AveOBestBet((BestBet)obj);
        }

        public IAveOBestBet this[Uri url]
        {
            get
            {
                BestBet bestBet = mBestBetCollection[url];
                if (bestBet == null)
                {
                    return null;
                }
                return new AveOBestBet(bestBet);
            }
        }

        public void Remove(IAveOBestBet bestBet)
        {
            if (bestBet == null)
            {
                throw new ArgumentNullException();
            }
            mBestBetCollection.Remove((bestBet as AveOBestBet).BestBet);
        }
    }
}
