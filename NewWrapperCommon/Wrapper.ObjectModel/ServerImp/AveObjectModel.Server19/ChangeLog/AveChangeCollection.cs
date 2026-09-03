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
using Microsoft.SharePoint;

namespace AvePoint.ObjectModel.Server19
{
    class AveChangeCollection : AveAbstractCommonCollection<IAveChange>, IAveChangeCollection
    {
        private SPChangeCollection mChangeCollection = null;
        private Dictionary<string, AveChange> changeCache = new Dictionary<string, AveChange>();

        public AveChangeCollection(SPChangeCollection changeCollection)
            : base(changeCollection)
        {
            if (changeCollection == null)
            {
                throw new ArgumentNullException();
            }

            mChangeCollection = changeCollection;
        }

        internal SPChangeCollection ChangeCollection
        {
            get { return mChangeCollection; }
        }

        #region Public Properties of SPChangeCollection

        public override int Count
        {
            get { return mChangeCollection.Count; }
        }

        public bool IncludeBeginning
        {
            get { return mChangeCollection.IncludesBeginning; }
        }

        public override IAveChange this[int index]
        {
            get { return new AveChange(mChangeCollection[index]); }
        }

        public IAveChangeToken LastChangeToken
        {
            get { return new AveChangeToken(mChangeCollection.LastChangeToken); }
        }

        #endregion

        #region The Internal Properties of SPChangeCollection
        public AveCollectionScope Scope
        {
            get { throw new NotImplementedException(); }
        }

        public Guid ScopeId
        {
            get { throw new NotImplementedException(); }
        }
        #endregion

        #region Implement AveAbstractCommonCollection

        internal AveChange CreateChangeByType(SPChange change)
        {
            if (change != null)
            {
                string key = string.Format("{0}:{1}:{2}", change.ChangeToken, change.ChangeType, change.Time);
                if (!changeCache.ContainsKey(key))
                {
                    lock (changeCache)
                    {
                        if (!changeCache.ContainsKey(key))
                        {
                            changeCache[key] = AveServerAssemblyInit.CreateElement(typeof(IAveChange), new object[] { change }) as AveChange;
                        }
                    }
                }
                return changeCache[key];
            }
            return null;
        }

        public override IEnumerator<IAveChange> GetEnumerator()
        {
            foreach (SPChange change in mChangeCollection)
            {
                yield return CreateChangeByType(change);
            }
        }

        protected override object CreatElementInstance(object t)
        {
            return CreateChangeByType(t as SPChange);
        }

        #endregion
    }
}
