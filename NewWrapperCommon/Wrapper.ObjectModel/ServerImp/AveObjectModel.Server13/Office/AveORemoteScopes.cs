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
using System.Collections;

namespace AvePoint.ObjectModel.Server13.Office
{
    class AveORemoteScopes : AveOScopesManager, IAveORemoteScopes
    {
        private RemoteScopes mRemoteScopes;

        public AveORemoteScopes(RemoteScopes remoteScopes)
            : base(remoteScopes)
        {
            mRemoteScopes = remoteScopes;
        }

        public AveORemoteScopes(IAveServiceContext serviceContext)
            : this(new RemoteScopes((serviceContext as AveServiceContext).ServiceContext))
        { }

        public IAveOScope GetScope(Uri siteUrl, string name)
        {
            Scope scope = mRemoteScopes.GetScope(siteUrl, name);
            if (scope == null)
            {
                return null;
            }
            return new AveOScope(scope);
        }

        public override IEnumerable GetDisplayGroupsForSite(Uri siteUrl)
        {
            return new Enumerable(typeof(IAveODisplayGroupInfo), mRemoteScopes.GetDisplayGroupsForSite(siteUrl));
        }

        public override IEnumerable GetUnusedScopesForSite(Uri siteUrl)
        {
            return new Enumerable(typeof(IAveOScopeInfo), mRemoteScopes.GetUnusedScopesForSite(siteUrl));
        }

        public override IEnumerable GetScopesForSite(Uri siteUrl)
        {
            return new Enumerable(typeof(IAveOScopeInfo), mRemoteScopes.GetScopesForSite(siteUrl));
        }

        public override IEnumerable GetSharedScopes()
        {
            return new Enumerable(typeof(IAveOScopeInfo), mRemoteScopes.GetSharedScopes());
        }

        private class Enumerable : AveAbstractCommonCollection
        {
            private Type mBaseType;

            public Enumerable(Type type, IEnumerable enumerable)
                : base(enumerable)
            {
                mBaseType = type;
            }

            internal override object CreatElementInstance(object obj)
            {
                return AveServerAssemblyInit.CreateElement(mBaseType, obj);
            }
        }
    }
}
