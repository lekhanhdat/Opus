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
using AvePoint.GCommon.Utility;

namespace AvePoint.ObjectModel.Server13
{
    class AveTermSetCollection : AveAbstractCommonCollection<IAveTermSet>, IAveTermSetCollection
    {
        private TermSetCollection mTermSetCollection;

        public AveTermSetCollection(TermSetCollection termSetCollection)
            : base(termSetCollection)
        {
            mTermSetCollection = termSetCollection;
        }

        protected override object CreatElementInstance(object t)
        {
            return new AveTermSet(t as TermSet);
        }

        public override int Count
        {
            get
            {
                return mTermSetCollection.Count;
            }
        }

        public override IAveTermSet this[int i]
        {
            get
            {
                if (i >= Count || i < 0)
                {
                    throw new AveException("Out of range.");
                }
                TermSet termSet = mTermSetCollection[i];
                if (termSet == null)
                {
                    return null;
                }
                return new AveTermSet(termSet);
            }
        }

        public IAveTermSet this[Guid id]
        {
            get
            {
                return new AveTermSet(mTermSetCollection[id]);
            }
        }

        public IAveTermSet this[string termSetName]
        {
            get
            {
                return new AveTermSet(mTermSetCollection[termSetName]);
            }
        }
    }
}
