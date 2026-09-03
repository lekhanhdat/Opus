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
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint;

namespace AvePoint.ObjectModel.Server13
{
    public class AveChangeToken : IAveChangeToken
    {
        private SPChangeToken mChangeToken = null;

        public AveChangeToken(string strChangeToken)
        {
            mChangeToken = new SPChangeToken(strChangeToken);
        }

        public AveChangeToken(AveCollectionScope scope, Guid scopeId, DateTime changeTime)
        {
            mChangeToken = new SPChangeToken((SPChangeCollection.CollectionScope)scope, scopeId, changeTime);
        }

        public AveChangeToken(SPChangeToken changeToken)
        {
            if (changeToken == null)
            {
                throw new ArgumentNullException();
            }

            mChangeToken = changeToken;
        }

        internal SPChangeToken ChangeToken
        {
            get { return mChangeToken; }
        }

        #region Public Properties of SPChangeToken

        public AveCollectionScope Scope
        {
            get { return (AveCollectionScope)mChangeToken.Scope; }
        }

        public Guid ScopeId
        {
            get { return mChangeToken.ScopeId; }
        }

        #endregion

        #region Override Methods

        public override string ToString()
        {
            return mChangeToken.ToString();
        }

        public override bool Equals(object obj)
        {
            return mChangeToken.Equals(obj);
        }

        public override int GetHashCode()
        {
 	        return mChangeToken.GetHashCode();
        }

        public static bool operator ==(AveChangeToken changeToken1, AveChangeToken changeToken2)
        {
            return changeToken1.mChangeToken == changeToken2.mChangeToken;
        }

        public static bool operator !=(AveChangeToken changeToken1, AveChangeToken changeToken2)
        {
            return changeToken1.mChangeToken != changeToken2.mChangeToken;
        }

        #endregion
    }
}
