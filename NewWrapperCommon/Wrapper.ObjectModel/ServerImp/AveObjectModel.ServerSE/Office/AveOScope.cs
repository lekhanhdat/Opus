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
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.ServerSE.Office
{
    class AveOScope : IAveOScope
    {
        private Scope mScope;
        private AveOScopeRuleCollection mRules;

        public AveOScope(Scope scope)
        {
            mScope = scope;
        }

        internal Scope Scope
        {
            get
            {
                return mScope;
            }
        }

        public IAveOScopeRuleCollection Rules
        {
            get
            {
                if (mRules == null)
                {
                    mRules = new AveOScopeRuleCollection(mScope.Rules);
                }
                return mRules;
            }
        }

        public int ID
        {
            get
            {
                return mScope.ID;
            }
        }

        public string Name
        {
            get
            {
                return mScope.Name;
            }
            set
            {
                mScope.Name = value;
            }
        }

        public bool IsShared
        {
            get { return (bool)AveAssemblyUtility.GetPropertyValue(mScope, "IsShared"); }
        }

        public AveScopeCompilationState CompilationState
        {
            get { return (AveScopeCompilationState)mScope.CompilationState; }
        }

        public int Count
        {
            get { return mScope.Count; }
        }

        public void Update()
        {
            mScope.Update();
        }

        public bool DisplayInAdminUI
        {
            get
            {
                return mScope.DisplayInAdminUI;
            }
            set
            {
                mScope.DisplayInAdminUI = value;
            }
        }

        public string LastModifiedBy
        {
            get
            {
                return mScope.LastModifiedBy;
            }
        }


        public string Description
        {
            get
            {
                return mScope.Description;
            }
            set
            {
                mScope.Description = value;
            }
        }

        public string AlternateResultsPage
        {
            get
            {
                return mScope.AlternateResultsPage;
            }
            set
            {
                mScope.AlternateResultsPage = value;
            }
        }
    }
}
