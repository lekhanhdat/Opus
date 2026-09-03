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



using AvePoint.ObjectModel.Server19.Search;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Search.Extended.Administration.Keywords;
using System;

namespace AvePoint.ObjectModel.Server19
{
    class AveFeaturedContent : IAveFeaturedContent
    {
        private FeaturedContent mFeaturedContent;
        private AveContextCollection mContexts;
        private AveSearchSettingGroup mGroup;
        private AveKeyword mKeyword;

        public AveFeaturedContent(FeaturedContent featuredContent)
        {
            mFeaturedContent = featuredContent;
        }

        internal FeaturedContent FeaturedContent
        {
            get
            {
                return mFeaturedContent;
            }
        }

        public short Position
        {
            get
            {
                return mFeaturedContent.Position;
            }
            set
            {
                mFeaturedContent.Position = value;
            }
        }

        public string Teaser
        {
            get
            {
                return mFeaturedContent.Teaser;
            }
            set
            {
                mFeaturedContent.Teaser = value;
            }
        }

        public string TeaserContentType
        {
            get
            {
                return mFeaturedContent.TeaserContentType;
            }
            set
            {
                mFeaturedContent.TeaserContentType = value;
            }
        }

        public Uri Uri
        {
            get
            {
                return mFeaturedContent.Uri;
            }
            set
            {
                mFeaturedContent.Uri = value;
            }
        }

        public void AttachContext(Wrapper.Common.Search.IAveContext cx)
        {
            mFeaturedContent.AttachContext((cx as Search.AveContext).Context);
        }

        public void DetachContext(Wrapper.Common.Search.IAveContext cx)
        {
            mFeaturedContent.DetachContext((cx as Search.AveContext).Context);
        }

        public void DetachContexts()
        {
            mFeaturedContent.DetachContexts();
        }

        public Wrapper.Common.Search.IAveContextCollection Contexts
        {
            get
            {
                if (mContexts == null)
                {
                    ContextCollection contexts = mFeaturedContent.Contexts;
                    if (contexts != null)
                    {
                        mContexts = new Search.AveContextCollection(contexts);
                    }
                }
                return mContexts;
            }
        }

        public DateTime? EndDate
        {
            get
            {
                return mFeaturedContent.EndDate;
            }
            set
            {
                mFeaturedContent.EndDate = value;
            }
        }

        public IAveSearchSettingGroup Group
        {
            get
            {
                if (mGroup == null)
                {
                    SearchSettingGroup settingGroup = mFeaturedContent.Group;
                    if (settingGroup != null)
                    {
                        mGroup = new AveSearchSettingGroup(settingGroup);
                    }
                }
                return mGroup;
            }
        }

        public IAveKeyword Keyword
        {
            get
            {
                if (mKeyword == null)
                {
                    Keyword keyword = mFeaturedContent.Keyword;
                    if (keyword != null)
                    {
                        mKeyword = new AveKeyword(keyword);
                    }
                }
                return mKeyword;
            }
        }

        public DateTime? StartDate
        {
            get
            {
                return mFeaturedContent.StartDate;
            }
            set
            {
                mFeaturedContent.StartDate = value;
            }
        }

        public string Description
        {
            get
            {
                return mFeaturedContent.Description;
            }
            set
            {
                mFeaturedContent.Description = value;
            }
        }

        public string Name
        {
            get
            {
                return mFeaturedContent.Name;
            }
            set
            {
                mFeaturedContent.Name = value;
            }
        }

        public long Id
        {
            get
            {
                return mFeaturedContent.Id;
            }
        }

        public DateTime LastChanged
        {
            get
            {
                return mFeaturedContent.LastChanged;
            }
            set
            {
                mFeaturedContent.LastChanged = value;
            }
        }

        public int CompareTo(IAveFeaturedContent other)
        {
            if (other == null)
            {
                return 1;
            }
            return mFeaturedContent.CompareTo((other as AveFeaturedContent).FeaturedContent);
        }
    }
}
