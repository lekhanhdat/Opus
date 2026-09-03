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
using AvePoint.Wrapper.Common.Office;
using System.Collections;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Common.Office
{
    class AveOScopeDisplayGroup : AveAbstractCommonCollection<IAveOScope>, IAveOScopeDisplayGroup
    {
        private IAveRequest m_Request;
        private IAveOScope m_Default;
        private List<int> m_ScopesId;

        public AveOScopeDisplayGroup()
        { }

        public AveOScopeDisplayGroup(Dictionary<string, object> groupProp, IAveRequest m_Request)
        {
            mListData = new List<IAveOScope>();
            this.m_Request = m_Request;
            base.DataCache.AddPropertyies(groupProp);
            InitScopeDisplayGroup();
        }

        private void InitScopeDisplayGroup()
        {
            List<Dictionary<string, object>> scopesProp = base.DataCache.PropertiesCache["Scopes"] as List<Dictionary<string, object>>;
            m_ScopesId = new List<int>();
            foreach (Dictionary<string, object> scopeProp in scopesProp)
            {
                AveOScope scope = new AveOScope(scopeProp, m_Request);
                if ((bool)scopeProp["Default"])
                {
                    m_Default = scope;
                }
                mListData.Add(scope);
                m_ScopesId.Add(scope.ID);
            }
            if (mListData.Count == 0)
            {
                m_Default = null;
            }

        }

        public int ID
        {
            get
            {
                return base.DataCache.GetProperty<int>("ID");
            }
        }

        public string Name
        {
            get
            {
                return base.DataCache.GetProperty<string>("Name");
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string Description
        {
            get
            {
                return base.DataCache.GetProperty<string>("Description");
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IAveOScope Default
        {
            get
            {
                return m_Default;
            }
            set
            {
                m_Default = value;
                base.DataCache.ChangedProperties["Default"] = Default.ID;
            }
        }

        public bool DisplayInAdminUI
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("DisplayInAdminUI"))
                {
                    return true;
                }
                return base.DataCache.GetProperty<bool>("DisplayInAdminUI");
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool Contains(IAveOScope scope)
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

        public void Add(IAveOScope scope)
        {
            mListData.Add(scope);
            m_ScopesId.Add(scope.ID);
            //if (!base.DataCache.ChangedProperties.ContainsKey("AddScopes"))
            //{
            //    base.DataCache.ChangedProperties["AddScopes"] = new List<string>();
            //}
            //(base.DataCache.ChangedProperties["AddScopes"] as List<string>).Add(scope.ID.ToString());
        }

        public void Update()
        {
            base.DataCache.ChangedProperties["UpdateScopes"] = m_ScopesId;
            m_Request.UpdateScopeDisplayGroup(this.ID, this.Name, base.DataCache.ChangedProperties);
        }

        public void Remove(IAveOScope scope)
        {
            IAveOScope tempScope = null;
            foreach (IAveOScope oneScope in mListData)
            {
                if (oneScope.Name.Equals(scope.Name))
                {
                    tempScope = oneScope;
                    break;
                }
            }
            if (tempScope != null)
            {
                mListData.Remove(tempScope);
                m_ScopesId.Remove(scope.ID);
            }
        }
    }
}
