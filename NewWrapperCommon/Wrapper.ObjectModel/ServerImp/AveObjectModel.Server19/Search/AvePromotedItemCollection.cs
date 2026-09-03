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
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Search.Extended.Administration.Keywords;
using System.Collections;

namespace AvePoint.ObjectModel.Server19
{
    class AvePromotedItemCollection : AveAbstractCommonCollection<IAvePromotedItem>, IAvePromotedItemCollection
    {
        private PromotedItemCollection mPromotedItemCollection;

        public AvePromotedItemCollection(PromotedItemCollection promotedItemCollection)
            : base(promotedItemCollection)
        {
            mPromotedItemCollection = promotedItemCollection;
        }

        public IAvePromotedDocument AddPromotedDocument(string documentId)
        {
            PromotedDocument promotedDocument = mPromotedItemCollection.AddPromotedDocument(documentId);
            if (promotedDocument != null)
            {
                return new AvePromotedDocument(promotedDocument);
            }
            return null;
        }

        public IAvePromotedLocation AddPromotedLocation(Uri uri)
        {
            PromotedLocation promotedLocation = mPromotedItemCollection.AddPromotedLocation(uri);
            if (promotedLocation != null)
            {
                return new AvePromotedLocation(promotedLocation);
            }
            return null;
        }

        public IAvePromotedExpression AddPromotedExpression(string fqlExpression)
        {
            PromotedExpression promotedExpression = mPromotedItemCollection.AddPromotedExpression(fqlExpression);
            if (promotedExpression != null)
            {
                return new AvePromotedExpression(promotedExpression);
            }
            return null;
        }

        protected override object CreatElementInstance(object t)
        {
            return AveServerAssemblyInit.CreateElement(typeof(IAvePromotedItem), t);
        }

        public override int Count
        {
            get { return mPromotedItemCollection.Count; }
        }

        public void Clear()
        {
            mPromotedItemCollection.Clear();
        }

        public IEnumerator<IAvePromotedLocation> GetPromotedLocationEnumerator()
        {
            IEnumerator<PromotedLocation> promotedLocations = mPromotedItemCollection.GetPromotedLocationEnumerator();
            return new Enumerator<IAvePromotedLocation>(promotedLocations);
        }

        public IEnumerator<IAvePromotedExpression> GetPromotedExpressionEnumerator()
        {
            IEnumerator<PromotedExpression> promotedExpression = mPromotedItemCollection.GetPromotedExpressionEnumerator();
            return new Enumerator<IAvePromotedExpression>(promotedExpression);
        }

        public IEnumerator<IAvePromotedDocument> GetPromotedDocumentEnumerator()
        {
            IEnumerator<PromotedDocument> promotedDocument = mPromotedItemCollection.GetPromotedDocumentEnumerator();
            return new Enumerator<IAvePromotedDocument>(promotedDocument);
        }

        private class Enumerator<C> : IEnumerator<C>, IEnumerable, IDisposable where C : IAvePromotedItem
        {
            protected IEnumerator mEnumerator;

            public Enumerator(IEnumerator enumerator)
            {
                mEnumerator = enumerator;
            }

            public C Current
            {
                get
                {
                    return (C)CreatElementInstance(mEnumerator.Current);
                }
            }

            private object CreatElementInstance(object obj)
            {
                if (obj != null)
                {
                    return AveServerAssemblyInit.CreateElement(typeof(C), obj);
                }
                return null;
            }

            public void Dispose()
            { }

            object IEnumerator.Current
            {
                get { return this.Current; }
            }

            public bool MoveNext()
            {
                return mEnumerator.MoveNext();
            }

            public void Reset()
            {
                mEnumerator.Reset();
            }

            public IEnumerator GetEnumerator()
            {
                return this;
            }
        }
    }
}
