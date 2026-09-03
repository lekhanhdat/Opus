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



using AvePoint.Wrapper.Common.Office;
using Microsoft.Office.Server.Search.Administration;
using System.Collections;

namespace AvePoint.ObjectModel.Server19.Office
{
    class AveOScopeDisplayGroup : AveAbstractCommonCollection, IAveOScopeDisplayGroup
    {
        private ScopeDisplayGroup mScopeDisplayGroup;
        private AveOScope mDefault;

        public AveOScopeDisplayGroup(ScopeDisplayGroup scopeDisplayGroup)
            : base(scopeDisplayGroup)
        {
            mScopeDisplayGroup = scopeDisplayGroup;
        }

        public int ID
        {
            get { return mScopeDisplayGroup.ID; }
        }

        public string Name
        {
            get
            {
                return mScopeDisplayGroup.Name;
            }
            set
            {
                mScopeDisplayGroup.Name = value;
            }
        }

        public string Description
        {
            get
            {
                return mScopeDisplayGroup.Description;
            }
            set
            {
                mScopeDisplayGroup.Description = value;
            }
        }

        public IAveOScope Default
        {
            get
            {
                if (mDefault == null)
                {
                    Scope scope = mScopeDisplayGroup.Default;
                    if (scope != null)
                    {
                        mDefault = new AveOScope(scope);
                    }
                }
                return mDefault;
            }
            set
            {
                mDefault = value as AveOScope;
                if (value != null)
                {
                    mScopeDisplayGroup.Default = (value as AveOScope).Scope;
                }
                else
                {
                    mScopeDisplayGroup.Default = null;
                }
            }
        }

        public bool Contains(IAveOScope scope)
        {
            return mScopeDisplayGroup.Contains((scope as AveOScope).Scope);
        }

        public void Add(IAveOScope scope)
        {
            mScopeDisplayGroup.Add((scope as AveOScope).Scope);
        }

        public void Update()
        {
            mScopeDisplayGroup.Update();
        }


        public bool DisplayInAdminUI
        {
            get
            {
                return mScopeDisplayGroup.DisplayInAdminUI;
            }
            set
            {
                mScopeDisplayGroup.DisplayInAdminUI = value;
            }
        }

        public void Remove(IAveOScope scope)
        {
            mScopeDisplayGroup.Remove((scope as AveOScope).Scope);
        }

        internal override object CreatElementInstance(object obj)
        {
            return new AveOScope(obj as Scope);
        }
    }
}
