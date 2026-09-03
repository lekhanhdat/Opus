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

namespace AvePoint.ObjectModel.ServerSE
{
    class AveAlternateUrlCollection : AvePersistedObject, IAveAlternateUrlCollection
    {
        private SPAlternateUrlCollection mAlternateUrlCollection;
        private List<IAveAlternateUrl> mAveAlternalUrls;

        public AveAlternateUrlCollection(SPAlternateUrlCollection alternateUrls)
            : base(alternateUrls)
        {
            mAlternateUrlCollection = alternateUrls;
            StoreData();
        }

        public AveAlternateUrlCollection(string name, IAveFarm farm)
            : this(new SPAlternateUrlCollection(name, (farm as AveFarm).Farm))
        { }

        internal void StoreData()
        {
            mAveAlternalUrls = new List<IAveAlternateUrl>();
            foreach (SPAlternateUrl spAlternateUrl in mAlternateUrlCollection)
            {
                mAveAlternalUrls.Add(new AveAlternateUrl(spAlternateUrl));
            }
        }

        internal SPAlternateUrlCollection AlternateUrlCollection
        {
            get
            {
                return mAlternateUrlCollection;
            }
        }

        #region IAveSPAlternateUrlCollection Members

        public IAveAlternateUrl GetResponseUrl(AveUrlZone urlZone)
        {
            SPAlternateUrl alternateUrl = mAlternateUrlCollection.GetResponseUrl((SPUrlZone)urlZone);
            if (alternateUrl == null)
            {
                return null;
            }
            return new AveAlternateUrl(alternateUrl);
        }

        public void Add(IAveAlternateUrl alternateUrl)
        {
            mAlternateUrlCollection.Add((alternateUrl as AveAlternateUrl).AlternateUrl);
        }

        public void Add(IAveAlternateUrl alternateUrl, bool fUpdate, bool throwIfExists)
        {
            object[] args = new object[] { (alternateUrl as AveAlternateUrl).AlternateUrl, fUpdate, throwIfExists };
            Type[] paramTypes = new Type[] { typeof(SPAlternateUrl), typeof(bool), typeof(bool) };
            AveAssemblyUtility.InvokeMethod(mAlternateUrlCollection, "Add", paramTypes, args);
        }

        public override void Update()
        {
            mAlternateUrlCollection.Update();
        }

        public override void Delete()
        {
            mAlternateUrlCollection.Delete();
        }

        public void SetResponseUrl(IAveAlternateUrl url)
        {
            mAlternateUrlCollection.SetResponseUrl((url as AveAlternateUrl).AlternateUrl);
        }

        public void UnsetResponseUrl(AveUrlZone zone)
        {
            mAlternateUrlCollection.UnsetResponseUrl((SPUrlZone)zone);
        }

        public void Delete(string incomingUrl)
        {
            mAlternateUrlCollection.Delete(incomingUrl);
        }

        public void Delete(string incomingUrl, bool update, bool throwIfNotFound)
        {
            object[] args = new object[] { incomingUrl, update, throwIfNotFound };
            Type[] paramTypes = new Type[] { typeof(string), typeof(bool), typeof(bool) };
            AveAssemblyUtility.InvokeMethod(mAlternateUrlCollection, "Delete", paramTypes, args);
        }

        #endregion

        public new IEnumerator GetEnumerator()
        {
            return mAveAlternalUrls.GetEnumerator();
        }

        public IAveAlternateUrl this[int index]
        {
            get
            {
                return mAveAlternalUrls[index];
            }
        }

        public void CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            int startIndex = index;
            for (int i = 0; i < this.Count; i++)
            {
                array.SetValue(this[i], startIndex + i);
            }
        }

        public int Count
        {
            get
            {
                return mAveAlternalUrls.Count;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public object SyncRoot
        {
            get
            {
                return this;
            }
        }

        IEnumerator<IAveAlternateUrl> IEnumerable<IAveAlternateUrl>.GetEnumerator()
        {
            return mAveAlternalUrls.GetEnumerator();
        }

        public IAveAlternateUrl this[string incomingUrl]
        {
            get
            {
                Uri uri = new Uri(incomingUrl);
                string leftPart = uri.GetLeftPart(UriPartial.Authority);
                if (!string.IsNullOrEmpty(leftPart))
                {
                    uri = new Uri(leftPart);
                }

                foreach (IAveAlternateUrl url in mAveAlternalUrls)
                {
                    if (url.Uri.Equals(uri))
                    {
                        return url;
                    }
                }
                return null;
            }
        }
    }
}
