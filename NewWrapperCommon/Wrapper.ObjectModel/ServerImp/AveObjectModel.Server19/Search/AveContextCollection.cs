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



using AvePoint.Wrapper.Common.Search;
using Microsoft.SharePoint.Search.Extended.Administration.Keywords;

namespace AvePoint.ObjectModel.Server19.Search
{
    class AveContextCollection : AveAbstractCommonCollection<IAveContext>, IAveContextCollection
    {
        private ContextCollection mContextCollection;

        public AveContextCollection(ContextCollection contextCollection)
            : base(contextCollection)
        {
            mContextCollection = contextCollection;
        }

        protected override object CreatElementInstance(object t)
        {
            return new AveContext((Context)t);
        }

        public override int Count
        {
            get
            {
                return mContextCollection.Count;
            }
        }

        public IAveContext AddContext(string context)
        {
            Context spcontext = mContextCollection.AddContext(context);
            if (spcontext != null)
            {
                return new AveContext(spcontext);
            }
            return null;
        }

        public bool ContainsContext(string context)
        {
            return mContextCollection.ContainsContext(context);
        }

        public IAveContext GetContext(string context)
        {
            Context spcontext = mContextCollection.GetContext(context);
            if (spcontext != null)
            {
                return new AveContext(spcontext);
            }
            return null;
        }

        public void RemoveContext(string context)
        {
            mContextCollection.RemoveContext(context);
        }

        public IAveContext this[string context]
        {
            get
            {
                Context spcontext = mContextCollection[context];
                if (spcontext != null)
                {
                    return new AveContext(spcontext);
                }
                return null;
            }
        }

        public void Clear()
        {
            mContextCollection.Clear();
        }
    }
}
