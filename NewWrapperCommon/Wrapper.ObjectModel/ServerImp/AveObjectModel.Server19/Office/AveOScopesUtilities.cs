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
using AvePoint.Wrapper.Common.Office;
using Microsoft.Office.Server.Search.Administration;

namespace AvePoint.ObjectModel.Server19.Office
{
    class AveOScopesUtilities : IAveOScopesUtilities
    {
        private readonly string mScopesUtilities_Type = "Microsoft.Office.Server.Search.Administration.ScopesUtilities";
        private readonly string mScopesUtilities_FriendlyScopeCompilationState_Method = "FriendlyScopeCompilationState";
        private object mScopesUtilities;

        public AveOScopesUtilities()
        {
            mScopesUtilities = AveAssemblyUtility.CreateInstance(mScopesUtilities_Type);
        }

        public AveOScopesUtilities(object scopesUtilities)
        {
            mScopesUtilities = scopesUtilities;
        }

        public string FriendlyScopeCompilationState(IAveOScope scope, IAveOScopesManager scopes, bool bPlainText, IAveOSearchServiceApplication searchApp)
        {
            Type[] paramTypes = new Type[] { typeof(Scope), typeof(ScopesManager), typeof(bool), typeof(SearchServiceApplication) };
            object[] args = new object[] { scope != null ? (scope as AveOScope).Scope : null, scopes != null ? (scopes as AveOScopesManager).ScopesManager : null, bPlainText, searchApp != null ? (searchApp as AveOSearchServiceApplication).SearchServiceApplication : null };
            return (string)AveAssemblyUtility.InvokeStaticMethod(mScopesUtilities_Type, mScopesUtilities_FriendlyScopeCompilationState_Method, paramTypes, args);
        }
    }
}
