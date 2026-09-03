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
using AvePoint.Wrapper.Common.Office;

namespace AvePoint.ObjectModel.Common.Office
{
    class AveOScopeCollection : AveAbstractCommonCollection<IAveOScope>, IAveOScopeCollection
    {
        private IAveRequest m_Request;

        public AveOScopeCollection(IAveRequest m_Request)
        {
            this.m_Request = m_Request;
            mListData = new List<IAveOScope>();
        }

        public IAveOScope Create(string name, string description, Uri owningSiteUrl, bool displayInAdminUI, string alternateResultsPage, AveScopeCompilationType compilationType)
        {
            Dictionary<string, object> newScopeProp = m_Request.CreateScope(name, description, owningSiteUrl, displayInAdminUI, alternateResultsPage, compilationType.ToString(), string.Empty);
            if (newScopeProp.Count == 0)
            {
                throw new Exception("Create scope failed.");
            }
            IAveOScope scope = new AveOScope(newScopeProp, m_Request);
            mListData.Add(scope);
            return scope;
        }

        public IAveOScope Create(string name, string description, Uri owningSiteUrl, bool displayInAdminUI, string alternateResultsPage, AveScopeCompilationType compilationType, string filter)
        {
            throw new NotImplementedException();
        }

        internal void AddScope(IAveOScope scope)
        {
            mListData.Add(scope);
        }

        internal bool ContainsScope(IAveOScope scope)
        {
            IAveOScope findedScope = mListData.Find(
                   delegate(IAveOScope scop)
                   {
                       return scop.Name.Equals(scope.Name);
                   });
            if (findedScope != null)
            {
                return true;
            }
            return false;
        }

        public IAveOScope this[int scopeId]
        {
            get
            {
                return mListData.Find(
                            delegate(IAveOScope scop)
                            {
                                return scop.ID == scopeId;
                            });
            }
        }
    }
}
