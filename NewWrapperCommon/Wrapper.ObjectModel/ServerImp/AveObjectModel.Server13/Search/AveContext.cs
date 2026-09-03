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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AvePoint.ObjectModel.Server13.Search
{
    class AveContext : IAveContext
    {
        private Context mContext;
        private AveAggregateContextExpression mContextExpression;
        private AveSearchSettingGroup mGroup;
        private Collection<Wrapper.Common.IAveSearchSetting> mSearchSettings;

        public AveContext(Context context)
        {
            mContext = context;
        }

        internal Context Context
        {
            get
            {
                return mContext;
            }
        }

        public int CompareTo(IAveContext other)
        {
            if (other == null)
            {
                return 1;
            }
            return mContext.CompareTo((other as AveContext).Context);
        }

        public string Description
        {
            get
            {
                return mContext.Description;
            }
            set
            {
                mContext.Description = value;
            }
        }

        public string Name
        {
            get
            {
                return mContext.Name;
            }
            set
            {
                mContext.Name = value;
            }
        }

        public long Id
        {
            get
            {
                return mContext.Id;
            }
        }

        public DateTime LastChanged
        {
            get
            {
                return mContext.LastChanged;
            }
            set
            {
                mContext.LastChanged = value;
            }
        }

        public Wrapper.Common.IAveAggregateContextExpression AddAndExpression()
        {
            AggregateContextExpression aggregateContextExpression = mContext.AddAndExpression();
            if (aggregateContextExpression != null)
            {
                return new AveAggregateContextExpression(aggregateContextExpression);
            }
            return null;
        }

        public Wrapper.Common.IAveAggregateContextExpression AddNotExpression()
        {
            AggregateContextExpression aggregateContextExpression = mContext.AddNotExpression();
            if (aggregateContextExpression != null)
            {
                return new AveAggregateContextExpression(aggregateContextExpression);
            }
            return null;
        }

        public Wrapper.Common.IAveAggregateContextExpression AddOrExpression()
        {
            AggregateContextExpression aggregateContextExpression = mContext.AddOrExpression();
            if (aggregateContextExpression != null)
            {
                return new AveAggregateContextExpression(aggregateContextExpression);
            }
            return null;
        }

        public Wrapper.Common.IAveAggregateContextExpression ContextExpression
        {
            get
            {
                if (mContextExpression == null)
                {
                    AggregateContextExpression aggregateContextExpression = mContext.ContextExpression;
                    if (aggregateContextExpression != null)
                    {
                        mContextExpression = new AveAggregateContextExpression(aggregateContextExpression);
                    }
                }
                return mContextExpression;
            }
        }

        public Wrapper.Common.IAveSearchSettingGroup Group
        {
            get
            {
                if (mGroup == null)
                {
                    SearchSettingGroup searchSettingGroup = mContext.Group;
                    if (searchSettingGroup != null)
                    {
                        mGroup = new AveSearchSettingGroup(searchSettingGroup);
                    }
                }
                return mGroup;
            }
        }

        public ICollection<Wrapper.Common.IAveSearchSetting> SearchSettings
        {
            get
            {
                if (mSearchSettings == null)
                {
                    ICollection<SearchSetting> searchSettings = mContext.SearchSettings;
                    if (searchSettings != null)
                    {
                        mSearchSettings = new Collection<Wrapper.Common.IAveSearchSetting>();
                        foreach (SearchSetting searchSetting in searchSettings)
                        {
                            mSearchSettings.Add(new AveSearchSetting(searchSetting));
                        }
                    }
                }
                return mSearchSettings;
            }
        }
    }
}
