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



using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Search.Extended.Administration.Keywords;
using AvePoint.Wrapper.Common.Search;
using AvePoint.ObjectModel.Server19.Search;
using System;

namespace AvePoint.ObjectModel.Server19
{
    class AveBestBet : IAveBestBet
    {
        private BestBet mBestBet;
        private AveContextCollection mContexts;
        private AveSearchSettingGroup mGroup;
        private AveKeyword mKeyword;

        public AveBestBet(BestBet bestBet)
        {
            mBestBet = bestBet;
        }

        internal BestBet BestBet
        {
            get
            {
                return mBestBet;
            }
        }

        public IAveContextCollection Contexts
        {
            get
            {
                if (mContexts == null)
                {
                    ContextCollection contextCollection = mBestBet.Contexts;
                    if (contextCollection != null)
                    {
                        mContexts = new AveContextCollection(contextCollection);
                    }
                }
                return mContexts;
            }
        }

        public short Position
        {
            get
            {
                return mBestBet.Position;
            }
            set
            {
                mBestBet.Position = value;
            }
        }

        public string Teaser
        {
            get
            {
                return mBestBet.Teaser;
            }
            set
            {
                mBestBet.Teaser = value;
            }
        }

        public string TeaserContentType
        {
            get
            {
                return mBestBet.TeaserContentType;
            }
            set
            {
                mBestBet.TeaserContentType = value;
            }
        }

        public Uri Uri
        {
            get
            {
                return mBestBet.Uri;
            }
            set
            {
                mBestBet.Uri = value;
            }
        }

        public void AttachContext(Wrapper.Common.Search.IAveContext cx)
        {
            mBestBet.AttachContext((cx as Search.AveContext).Context);
        }

        public void DetachContext(Wrapper.Common.Search.IAveContext cx)
        {
            mBestBet.DetachContext((cx as Search.AveContext).Context);
        }

        public void DetachContexts()
        {
            mBestBet.DetachContexts();
        }

        public DateTime? EndDate
        {
            get
            {
                return mBestBet.EndDate;
            }
            set
            {
                mBestBet.EndDate = value;
            }
        }

        public IAveSearchSettingGroup Group
        {
            get
            {
                if (mGroup == null)
                {
                    SearchSettingGroup settingGroup = mBestBet.Group;
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
                    Keyword keyword = mBestBet.Keyword;
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
                return mBestBet.StartDate;
            }
            set
            {
                mBestBet.StartDate = value;
            }
        }

        public string Description
        {
            get
            {
                return mBestBet.Description;
            }
            set
            {
                mBestBet.Description = value;
            }
        }

        public string Name
        {
            get
            {
                return mBestBet.Name;
            }
            set
            {
                mBestBet.Name = value;
            }
        }

        public long Id
        {
            get
            {
                return mBestBet.Id;
            }
        }

        public DateTime LastChanged
        {
            get
            {
                return mBestBet.LastChanged;
            }
            set
            {
                mBestBet.LastChanged = value;
            }
        }

        public int CompareTo(IAveBestBet other)
        {
            if (other == null || !(other is IAveBestBet))
            {
                return 1;
            }
            return mBestBet.CompareTo((other as AveBestBet).BestBet);
        }
    }
}
