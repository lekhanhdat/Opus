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
using System.Collections;
using Microsoft.SharePoint.Administration;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.ServerSE
{
    class AvePersistedTypeCollection : IAvePersistedTypeCollection,IDisposable
    {
        protected SPPersistedTypeCollection mPersistedTypeCollection;
        private AveFarm mFarm;

        public AvePersistedTypeCollection(SPPersistedTypeCollection persistedTypeCollection)
        {
            mPersistedTypeCollection = persistedTypeCollection;
        }

        public AvePersistedTypeCollection(IAveFarm farm, Type type)
        {
            string typeMapping = string.Empty;
            typeMapping = XmlConfiguration.GetTypeMapping(type.Name);
            mPersistedTypeCollection = new SPPersistedTypeCollection((farm as AveFarm).Farm, AveAssemblyUtility.GetGenerticType(type, typeMapping));
        }

        public IEnumerator GetEnumerator()
        {
            foreach (object currentObj in mPersistedTypeCollection)
            {
                if (currentObj != null)
                {
                    yield return AveServerAssemblyInit.CreateElement(typeof(IAvePersistedObject), currentObj);
                }
                else
                {
                    yield return null;
                }
            }
        }

        public IAveFarm Farm
        {
            get
            {
                if (mFarm == null)
                {
                    SPFarm farm = (SPFarm)AveAssemblyUtility.GetPropertyValue(mPersistedTypeCollection, "Farm");
                    if (farm != null)
                    {
                        mFarm = new AveFarm(farm);
                    }
                }
                return mFarm;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (mFarm != null)
            {
                mFarm.Dispose();
                mFarm = null;
            }
        }

        #endregion
    }

    class AvePersistedTypeCollection<T> : AvePersistedTypeCollection, IAvePersistedTypeCollection<T> where T : IAvePersistedObject
    {
        public AvePersistedTypeCollection(IEnumerable persistedTypeCollection)
            : base((SPPersistedTypeCollection)persistedTypeCollection)
        { }

        public AvePersistedTypeCollection(IAveFarm farm)
            : base(farm, typeof(T))
        { }

        public new IEnumerator<T> GetEnumerator()
        {
            return new Enumerator<T>(this);
        }

        internal virtual object CreateElementInstance(Type genericType, object persistedObject)
        {
            return AveServerAssemblyInit.CreateElement(genericType, persistedObject);
        }

        private class Enumerator<C> : IEnumerator<C>, IDisposable, IEnumerable where C : IAvePersistedObject
        {
            private IEnumerator mEnumerator;
            private AvePersistedTypeCollection<C> mPersistedTypes;

            public Enumerator(AvePersistedTypeCollection<C> persistedTypes)
            {
                mPersistedTypes = persistedTypes;
                mEnumerator = persistedTypes.mPersistedTypeCollection.GetEnumerator();
            }

            public C Current
            {
                get
                {
                    object obj = mPersistedTypes.CreateElementInstance(typeof(C), mEnumerator.Current);
                    return (C)obj;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }

            public bool MoveNext()
            {
                return mEnumerator.MoveNext();
            }

            public void Reset()
            {
                mEnumerator.Reset();
            }

            public void Dispose()
            { }

            public IEnumerator GetEnumerator()
            {
                return this;
            }
        }
    }
}
