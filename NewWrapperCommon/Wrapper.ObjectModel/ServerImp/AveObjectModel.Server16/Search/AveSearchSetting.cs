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
using AvePoint.ObjectModel.Server16.Search;

namespace AvePoint.ObjectModel.Server16
{
    class AveSearchSetting : IAveSearchSetting
    {
        private SearchSetting mSearchSetting;
        private AveContextCollection mContexts;
        private AveSearchSettingGroup mGroup;
        private AveKeyword mKeyword;

        public AveSearchSetting(SearchSetting searchSetting)
        {
            mSearchSetting = searchSetting;
        }

        public void AttachContext(Wrapper.Common.Search.IAveContext cx)
        {
            mSearchSetting.AttachContext((cx as ObjectModel.Server16.Search.AveContext).Context);
        }

        public void DetachContext(Wrapper.Common.Search.IAveContext cx)
        {
            mSearchSetting.DetachContext((cx as ObjectModel.Server16.Search.AveContext).Context);
        }

        public void DetachContexts()
        {
            mSearchSetting.DetachContexts();
        }

        public Wrapper.Common.Search.IAveContextCollection Contexts
        {
            get
            {
                if (mContexts == null)
                {
                    ContextCollection contextCollection = mSearchSetting.Contexts;
                    if (contextCollection != null)
                    {
                        mContexts = new AveContextCollection(contextCollection);
                    }
                }
                return mContexts;
            }
        }

        public DateTime? EndDate
        {
            get
            {
                return mSearchSetting.EndDate;
            }
            set
            {
                mSearchSetting.EndDate = value;
            }
        }

        public IAveSearchSettingGroup Group
        {
            get
            {
                if (mGroup == null)
                {
                    SearchSettingGroup settingGroup = mSearchSetting.Group;
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
                    Keyword keyword = mSearchSetting.Keyword;
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
                return mSearchSetting.StartDate;
            }
            set
            {
                mSearchSetting.StartDate = value;
            }
        }

        public string Description
        {
            get
            {
                return mSearchSetting.Description;
            }
            set
            {
                mSearchSetting.Description = value;
            }
        }

        public string Name
        {
            get
            {
                return mSearchSetting.Name;
            }
            set
            {
                mSearchSetting.Name = value;
            }
        }

        public long Id
        {
            get { return mSearchSetting.Id; }
        }

        public DateTime LastChanged
        {
            get
            {
                return mSearchSetting.LastChanged;
            }
            set
            {
                mSearchSetting.LastChanged = value;
            }
        }
    }
}
