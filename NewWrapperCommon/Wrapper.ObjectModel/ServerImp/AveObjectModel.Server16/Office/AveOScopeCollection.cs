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
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server16.Office
{
    class AveOScopeCollection : AveAbstractCommonCollection, IAveOScopeCollection
    {
        private ScopeCollection mScopeCollection;

        public AveOScopeCollection(ScopeCollection scopeCollection)
            : base(scopeCollection)
        {
            mScopeCollection = scopeCollection;
        }

        public IAveOScope this[int index]
        {
            get
            {
                Scope scope = mScopeCollection[index];
                if (scope == null)
                {
                    return null;
                }
                return new AveOScope(scope);
            }
        }

        internal override object CreatElementInstance(object t)
        {
            return new AveOScope(t as Scope);
        }

        public int Count
        {
            get { return mScopeCollection.Count; }
        }

        public IAveOScope Create(string name, string description, Uri owningSiteUrl, bool displayInAdminUI, string alternateResultsPage, AveScopeCompilationType compilationType)
        {
            return new AveOScope(mScopeCollection.Create(name,description,owningSiteUrl,displayInAdminUI,alternateResultsPage,(ScopeCompilationType)compilationType));
        }

        public IAveOScope Create(string name, string description, Uri owningSiteUrl, bool displayInAdminUI, string alternateResultsPage, AveScopeCompilationType compilationType, string filter)
        {
            return new AveOScope(mScopeCollection.Create(name,description,owningSiteUrl,displayInAdminUI,alternateResultsPage,(ScopeCompilationType)compilationType,filter));
        }
    }
}
