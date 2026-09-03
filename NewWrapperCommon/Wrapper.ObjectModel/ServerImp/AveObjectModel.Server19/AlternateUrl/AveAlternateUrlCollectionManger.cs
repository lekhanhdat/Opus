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
using Microsoft.SharePoint.Administration;
using AvePoint.Wrapper.Common;
using System.Collections;

namespace AvePoint.ObjectModel.Server19
{
    class AveAlternateUrlCollectionManager : AvePersistedChildCollection<IAveAlternateUrlCollection>, IAveAlternateUrlCollectionManager
    {
        private SPAlternateUrlCollectionManager mAlternateUrlsManager;

        public AveAlternateUrlCollectionManager(SPAlternateUrlCollectionManager alternateUrlsManager)
            : base(alternateUrlsManager)
        {
            mAlternateUrlsManager = alternateUrlsManager;
        }

        public IAveAlternateUrl GetResponseUrl(AveUrlZone urlZone)
        {
            throw new NotImplementedException();
        }

        public void Add(IAveAlternateUrlCollection aveAlternateUrls)
        {
            mAlternateUrlsManager.Add((aveAlternateUrls as AveAlternateUrlCollection).AlternateUrlCollection);
        }

        public override int Count
        {
            get { return mAlternateUrlsManager.Count; }
        }

        public new IEnumerator<IAveAlternateUrl> GetEnumerator()
        {
            return new Enumerator<IAveAlternateUrl>((mAlternateUrlsManager as IEnumerable<SPAlternateUrl>).GetEnumerator());
        }

        internal override object CreateElementInstance(Type genericType, object persistedObject)
        {
            Type instanceType = AveServerAssemblyInit.GetAveType(typeof(IAveAlternateUrlCollection), persistedObject);
            if (instanceType == typeof(AveAlternateUrl) && ((SPAlternateUrl)persistedObject).Collection != null)
            {
                return new AveAlternateUrlCollection(((SPAlternateUrl)persistedObject).Collection);
            }
            else
            {
                return base.CreateElementInstance(genericType, persistedObject);
            }
        }

        private class Enumerator<C> : IEnumerator<C>, IEnumerable, IDisposable where C : IAveAlternateUrl
        {
            private IEnumerator<SPAlternateUrl> mEnumerator;

            public Enumerator(IEnumerator<SPAlternateUrl> enumerator)
            {
                mEnumerator = enumerator;
            }

            public C Current
            {
                get
                {
                    object obj = new AveAlternateUrl(mEnumerator.Current);
                    return (C)obj;
                }
            }

            public void Dispose()
            { }

            object IEnumerator.Current
            {
                get { return mEnumerator.Current; }
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
                return mEnumerator;
            }
        }
    }
}
