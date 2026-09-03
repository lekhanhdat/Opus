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
using System;
using AvePoint.ObjectModel.Server13.Search;

namespace AvePoint.ObjectModel.Server13
{
    class AveSynonym : IAveSynonym
    {
        private Synonym mSynonym;
        private AveContextCollection mContexts;
        private AveSearchSettingGroup mGroup;
        private AveKeyword mKeyword;

        public AveSynonym(Synonym synonym)
        {
            mSynonym = synonym;
        }

        internal Synonym Synonym
        {
            get
            {
                return mSynonym;
            }
        }

        public AveSynonymExpansionType ExpansionType
        {
            get
            {
                return (AveSynonymExpansionType)mSynonym.ExpansionType;
            }
        }

        public string Term
        {
            get
            {
                return mSynonym.Term;
            }
            set
            {
                mSynonym.Term = value;
            }
        }

        public void AttachContext(Wrapper.Common.Search.IAveContext cx)
        {
            mSynonym.AttachContext((cx as Server13.Search.AveContext).Context);
        }

        public void DetachContext(Wrapper.Common.Search.IAveContext cx)
        {
            mSynonym.DetachContext((cx as Server13.Search.AveContext).Context);
        }

        public void DetachContexts()
        {
            mSynonym.DetachContexts();
        }

        public Wrapper.Common.Search.IAveContextCollection Contexts
        {
            get
            {
                if (mContexts == null)
                {
                    ContextCollection contexts = mSynonym.Contexts;
                    if (contexts != null)
                    {
                        mContexts = new AveContextCollection(contexts);
                    }
                }
                return mContexts;
            }
        }

        public DateTime? EndDate
        {
            get
            {
                return mSynonym.EndDate;
            }
            set
            {
                mSynonym.EndDate = value;
            }
        }

        public IAveSearchSettingGroup Group
        {
            get
            {
                if (mGroup == null)
                {
                    SearchSettingGroup settingGroup = mSynonym.Group;
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
                    Microsoft.SharePoint.Search.Extended.Administration.Keywords.Keyword keyword = mSynonym.Keyword;
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
                return mSynonym.StartDate;
            }
            set
            {
                mSynonym.StartDate = value;
            }
        }

        public string Description
        {
            get
            {
                return mSynonym.Description;
            }
            set
            {
                mSynonym.Description = value;
            }
        }

        public string Name
        {
            get
            {
                return mSynonym.Name;
            }
            set
            {
                mSynonym.Name = value;
            }
        }

        public long Id
        {
            get
            {
                return mSynonym.Id;
            }
        }

        public DateTime LastChanged
        {
            get
            {
                return mSynonym.LastChanged;
            }
            set
            {
                mSynonym.LastChanged = value;
            }
        }

        public int CompareTo(IAveSynonym other)
        {
            if (other == null || !(other is IAveSynonym))
            {
                return 1;
            }
            return mSynonym.CompareTo((other as AveSynonym).Synonym);
        }
    }
}
