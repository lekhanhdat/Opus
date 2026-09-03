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
using Microsoft.SharePoint.Search.Extended.Administration.Keywords;

namespace AvePoint.ObjectModel.Server19
{
    class AveContextExpression : IAveContextExpression
    {
        private ContextExpression mContextExpression;
        private AveAggregateContextExpression mParent;
        private AvePoint.ObjectModel.Server19.Search.AveContext mContext;

        public AveContextExpression(ContextExpression contextExpression)
        {
            mContextExpression = contextExpression;
        }

        public AvePoint.Wrapper.Common.Search.IAveContext Context
        {
            get
            {
                if (mContext == null)
                {
                    Microsoft.SharePoint.Search.Extended.Administration.Keywords.Context context = mContextExpression.Context;
                    if (context != null)
                    {
                        mContext = new AvePoint.ObjectModel.Server19.Search.AveContext(context);
                    }
                }
                return mContext;
            }
        }

        public IAveAggregateContextExpression Parent
        {
            get
            {
                if (mParent == null)
                {
                    AggregateContextExpression aggregateContextExpression = mContextExpression.Parent;
                    if (aggregateContextExpression != null)
                    {
                        mParent = new AveAggregateContextExpression(aggregateContextExpression);
                    }
                }
                return mParent;
            }
        }

        public AveExpressionTypes Type
        {
            get
            {
                return (AveExpressionTypes)mContextExpression.Type;
            }
        }

        public string Description
        {
            get
            {
                return mContextExpression.Description;
            }
            set
            {
                mContextExpression.Description = value;
            }
        }

        public string Name
        {
            get
            {
                return mContextExpression.Name;
            }
            set
            {
                mContextExpression.Name = value;
            }
        }

        public long Id
        {
            get { return mContextExpression.Id; }
        }

        public DateTime LastChanged
        {
            get
            {
                return mContextExpression.LastChanged;
            }
            set
            {
                mContextExpression.LastChanged = value;
            }
        }
    }
}
