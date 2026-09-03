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

namespace AvePoint.ObjectModel.Server16
{
    class AveAggregateContextExpression : AveAbstractCommonCollection<IAveContextExpression>, IAveAggregateContextExpression
    {
        private AggregateContextExpression mAggregateContextExpression;
        private AveAggregateContextExpression mParent;
        private AvePoint.ObjectModel.Server16.Search.AveContext mContent;

        public AveAggregateContextExpression(AggregateContextExpression aggregateContextExpression)
            : base(aggregateContextExpression)
        {
            mAggregateContextExpression = aggregateContextExpression;
        }

        public IAveAggregateContextExpression AddAndExpression()
        {
            AggregateContextExpression aggregateContextExpression = mAggregateContextExpression.AddAndExpression();
            if (aggregateContextExpression != null)
            {
                return new AveAggregateContextExpression(aggregateContextExpression);
            }
            return null;
        }

        public IAveKeyValueAtomicContextExpression AddMatchExpression(string key, string value)
        {
            KeyValueAtomicContextExpression keyValueAtomicContextExpression = mAggregateContextExpression.AddMatchExpression(key,value);
            if (keyValueAtomicContextExpression != null)
            {
                return new AveKeyValueAtomicContextExpression(keyValueAtomicContextExpression);
            }
            return null;
        }

        public IAveAggregateContextExpression AddNotExpression()
        {
            AggregateContextExpression aggregateContextExpression = mAggregateContextExpression.AddNotExpression();
            if (aggregateContextExpression != null)
            {
                return new AveAggregateContextExpression(aggregateContextExpression);
            }
            return null;
        }

        public IAveAggregateContextExpression AddOrExpression()
        {
            AggregateContextExpression aggregateContextExpression = mAggregateContextExpression.AddOrExpression();
            if (aggregateContextExpression != null)
            {
                return new AveAggregateContextExpression(aggregateContextExpression);
            }
            return null;
        }

        public AvePoint.Wrapper.Common.Search.IAveContext Context
        {
            get
            {
                if (mContent == null)
                {
                    Microsoft.SharePoint.Search.Extended.Administration.Keywords.Context context = mAggregateContextExpression.Context;
                    if (context != null)
                    {
                        mContent = new AvePoint.ObjectModel.Server16.Search.AveContext(context);
                    }
                }
                return mContent;
            }
        }

        public IAveAggregateContextExpression Parent
        {
            get 
            {
                if (mParent == null)
                {
                    AggregateContextExpression aggregateContextExpression = mAggregateContextExpression.Parent;
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
                return (AveExpressionTypes)mAggregateContextExpression.Type;
            }
        }

        public string Description
        {
            get
            {
                return mAggregateContextExpression.Description;
            }
            set
            {
                mAggregateContextExpression.Description = value;
            }
        }

        public string Name
        {
            get
            {
                return mAggregateContextExpression.Name;
            }
            set
            {
                mAggregateContextExpression.Name = value;
            }
        }

        public long Id
        {
            get { return mAggregateContextExpression.Id; }
        }

        public DateTime LastChanged
        {
            get
            {
                return mAggregateContextExpression.LastChanged;
            }
            set
            {
                mAggregateContextExpression.LastChanged = value;
            }
        }

        protected override object CreatElementInstance(object t)
        {
            return AveServerAssemblyInit.CreateElement(typeof(IAveContextExpression), t);
        }

        public override int Count
        {
            get { return mAggregateContextExpression.Count; }
        }

        public void Clear()
        {
            mAggregateContextExpression.Clear();
        }
    }
}
