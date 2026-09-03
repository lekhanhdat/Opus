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
using Microsoft.SharePoint.Administration;

namespace AvePoint.ObjectModel.ServerSE
{
    class AvePrefixCollection : AveAbstractCommonCollection<IAvePrefix>, IAvePrefixCollection
    {
        private SPPrefixCollection mPrefixCollection = null;

        public AvePrefixCollection(SPPrefixCollection prefixCollection)
            : base(prefixCollection)
        {
            this.mPrefixCollection = prefixCollection;
        }

        protected override object CreatElementInstance(object t)
        {
            return new AvePrefix(t as SPPrefix);
        }

        #region IAvePrefixCollection Members

        public IAvePrefix Add(string prefix, AvePrefixType prefixType)
        {
            return new AvePrefix(mPrefixCollection.Add(prefix, (SPPrefixType)prefixType));
        }

        public void Delete(string strPrefix)
        {
            mPrefixCollection.Delete(strPrefix);
        }

        public override IAvePrefix this[int index]
        {
            get
            {
                return new AvePrefix(mPrefixCollection[index]);
            }
        }

        public override int Count
        {
            get { return mPrefixCollection.Count; }
        }

        public bool Contains(string strPrefix)
        {
            if (strPrefix == null)
            {
                return false;
            }
            return mPrefixCollection.Contains(strPrefix);
        }

        #endregion
    }
}
