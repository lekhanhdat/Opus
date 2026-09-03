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

namespace AvePoint.ObjectModel.Server13.Office
{
    class AveOScopeRuleCollection : AveAbstractCommonCollection, IAveOScopeRuleCollection
    {
        private ScopeRuleCollection mScopeRuleCollection;

        public AveOScopeRuleCollection(ScopeRuleCollection scopeRuleCollection)
            : base(scopeRuleCollection)
        {
            mScopeRuleCollection = scopeRuleCollection;
        }

        public IAveOScopeRule this[int index]
        {
            get
            {
                ScopeRule scopeRule = mScopeRuleCollection[index];
                if (scopeRule == null)
                {
                    return null;
                }
                return (IAveOScopeRule)AveServerAssemblyInit.CreateElement(typeof(IAveOScopeRule), scopeRule);
            }
        }

        internal override object CreatElementInstance(object t)
        {
            return AveServerAssemblyInit.CreateElement(typeof(IAveOScopeRule), t);
        }

        public int Count
        {
            get { return mScopeRuleCollection.Count; }
        }

        public IAveOUrlScopeRule CreateUrlRule(AveScopeRuleFilterBehavior filterBehavior, AveUrlScopeRuleType urlRuleType, string matchingText)
        {
            return new AveOUrlScopeRule(mScopeRuleCollection.CreateUrlRule((ScopeRuleFilterBehavior)filterBehavior,(UrlScopeRuleType)urlRuleType,matchingText));
        }
    }
}
