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

namespace AvePoint.ObjectModel.ServerSE.Office
{
    class AveOScopeDisplayGroupCollection : AveAbstractCommonCollection, IAveOScopeDisplayGroupCollection
    {
        private ScopeDisplayGroupCollection mScopeDisplayGroupCollection;

        public AveOScopeDisplayGroupCollection(ScopeDisplayGroupCollection scopeDisplayGroupCollection)
            : base(scopeDisplayGroupCollection)
        {
            mScopeDisplayGroupCollection = scopeDisplayGroupCollection;
        }

        public IAveOScopeDisplayGroup this[int index]
        {
            get
            {
                ScopeDisplayGroup scopeDisplayGroup = mScopeDisplayGroupCollection[index];
                if (scopeDisplayGroup == null)
                {
                    return null;
                }
                return new AveOScopeDisplayGroup(scopeDisplayGroup);
            }
        }

        internal override object CreatElementInstance(object t)
        {
            return new AveOScopeDisplayGroup(t as ScopeDisplayGroup);
        }

        public int Count
        {
            get { return mScopeDisplayGroupCollection.Count; }
        }

        public IAveOScopeDisplayGroup Create(string name, string description, Uri owningSiteUrl, bool displayInAdminUI)
        {
            return new AveOScopeDisplayGroup(mScopeDisplayGroupCollection.Create(name, description, owningSiteUrl, displayInAdminUI));
        }
    }
}
