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
using System;

namespace AvePoint.ObjectModel.ServerSE.Office
{
    abstract class AveOScopesManager : IAveOScopesManager
    {
        private ScopesManager mScopesManager;
        private AveOScopeDisplayGroupCollection mAllDisplayGroups;
        private AveOScopeCollection mAllScopes;

        public AveOScopesManager(ScopesManager scopesManager)
        {
            mScopesManager = scopesManager;
        }

        internal ScopesManager ScopesManager
        {
            get
            {
                return mScopesManager;
            }
        }

        public IAveOScopeDisplayGroupCollection AllDisplayGroups
        {
            get
            {
                if (mAllDisplayGroups == null)
                {
                    ScopeDisplayGroupCollection allDisplayGroups = mScopesManager.AllDisplayGroups;
                    if (allDisplayGroups != null)
                    {
                        mAllDisplayGroups = new AveOScopeDisplayGroupCollection(allDisplayGroups);
                    }
                }
                return mAllDisplayGroups;
            }
        }

        public IAveOScopeCollection AllScopes
        {
            get
            {
                if (mAllScopes == null)
                {
                    ScopeCollection allScopes = mScopesManager.AllScopes;
                    if (allScopes != null)
                    {
                        mAllScopes = new AveOScopeCollection(allScopes);
                    }
                }
                return mAllScopes;
            }
        }

        public abstract IEnumerable GetDisplayGroupsForSite(Uri siteUrl);

        public abstract IEnumerable GetUnusedScopesForSite(Uri siteUrl);

        public abstract IEnumerable GetScopesForSite(Uri siteUrl);

        public abstract IEnumerable GetSharedScopes();
    }
}
