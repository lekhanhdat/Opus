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
using Microsoft.SharePoint.Taxonomy;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveTermStoreCollection : AveAbstractCommonCollection<IAveTermStore>, IAveTermStoreCollection
    {
        private TermStoreCollection mTermStoreCollection;

        public AveTermStoreCollection(TermStoreCollection termStoreCollection)
            : base(termStoreCollection)
        {
            mTermStoreCollection = termStoreCollection;
        }

        protected override object CreatElementInstance(object t)
        {
            return new AveTermStore(t as TermStore);
        }

        public override int Count
        {
            get { return mTermStoreCollection.Count; }
        }

        #region IAveTermStoreCollection Members

        public IAveTermStore this[Guid id]
        {
            get
            {
                return new AveTermStore(mTermStoreCollection[id]);
            }
        }

        public IAveTermStore this[string termName]
        {
            get
            {
                return new AveTermStore(mTermStoreCollection[termName]);
            }
        }

        public override IAveTermStore this[int index]
        {
            get
            {
                TermStore termStore = mTermStoreCollection[index];
                if (termStore == null)
                {
                    return null;
                }
                return new AveTermStore(termStore);
            }
        }

        #endregion
    }
}
