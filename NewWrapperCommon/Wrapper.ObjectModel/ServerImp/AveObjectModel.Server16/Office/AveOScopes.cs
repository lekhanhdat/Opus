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
using Microsoft.Office.Server.Search.Administration;
using System.Collections;

namespace AvePoint.ObjectModel.Server16.Office
{
    class AveOScopes : AveOScopesManager, IAveOScopes
    {
        private Scopes mScopes;

        public AveOScopes(Scopes scopes)
            : base(scopes)
        {
            mScopes = scopes;
        }

        public AveOScopes(IAveOSearchServiceApplication aveOSearchServiceApplication)
            : this(new Scopes((aveOSearchServiceApplication as AveOSearchServiceApplication).SearchServiceApplication))
        { }

        public AveOScopes(IAveOSearchContext aveOSearchContext)
            : this(new Scopes((aveOSearchContext as AveOSearchContext).SearchContext))
        { }

        public override IEnumerable GetDisplayGroupsForSite(Uri siteUrl)
        {
            return new Enumerable(typeof(IAveOScopes), mScopes.GetDisplayGroupsForSite(siteUrl));
        }

        public override IEnumerable GetUnusedScopesForSite(Uri siteUrl)
        {
            return new Enumerable(typeof(IAveOScopes), mScopes.GetUnusedScopesForSite(siteUrl));
        }

        public override IEnumerable GetScopesForSite(Uri siteUrl)
        {
            return new Enumerable(typeof(IAveOScopes), mScopes.GetScopesForSite(siteUrl));
        }

        public override IEnumerable GetSharedScopes()
        {
            return new Enumerable(typeof(IAveOScopes), mScopes.GetSharedScopes());
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
