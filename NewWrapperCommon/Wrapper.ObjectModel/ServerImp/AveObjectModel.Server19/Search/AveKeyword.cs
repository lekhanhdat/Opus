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

namespace AvePoint.ObjectModel.Server19
{
    class AveKeyword : IAveKeyword
    {
        private Keyword mKeyword;
        private AveBestBetCollection mBestBets;
        private AveFeaturedContentCollection mFeaturedContent;
        private AveSearchSettingGroup mGroup;
        private AvePromotionCollection mPromotions;
        private AveSynonymCollection mSynonyms;

        public AveKeyword(Keyword keyword)
        {
            mKeyword = keyword;
        }

        internal Keyword Keyword
        {
            get
            {
                return mKeyword;
            }
        }

        public IAveFeaturedContent AddFeaturedContent(string featuredContentName)
        {
            FeaturedContent featuredContent = mKeyword.AddFeaturedContent(featuredContentName);
            if (featuredContent != null)
            {
                return new AveFeaturedContent(featuredContent);
            }
            return null;
        }

        public IAvePromotion AddPromotion(string promotionName)
        {
            Promotion promotion = mKeyword.AddPromotion(promotionName);
            if (promotion == null)
            {
                return null;
            }
            return new AvePromotion(promotion);
        }

        public IAveBestBetCollection BestBets
        {
            get
            {
                if (mBestBets == null)
                {
                    BestBetCollection bestBets = mKeyword.BestBets;
                    if (bestBets != null)
                    {
                        mBestBets = new AveBestBetCollection(bestBets);
                        return mBestBets;
                    }
                }
                return mBestBets;
            }
        }

        public string Definition
        {
            get
            {
                return mKeyword.Definition;
            }
            set
            {
                mKeyword.Definition = value;
            }
        }

        public IAveFeaturedContentCollection FeaturedContent
        {
            get
            {
                if (mFeaturedContent == null)
                {

                    FeaturedContentCollection featuredContentCollection = mKeyword.FeaturedContent;
                    if (featuredContentCollection != null)
                    {
                        mFeaturedContent = new AveFeaturedContentCollection(featuredContentCollection);
                    }
                }
                return mFeaturedContent;
            }
        }

        public IAveSearchSettingGroup Group
        {
            get
            {
                if (mGroup == null)
                {

                    SearchSettingGroup group = mKeyword.Group;
                    if (group != null)
                    {
                        mGroup = new AveSearchSettingGroup(group);
                    }
                }
                return mGroup;
            }
        }

        public IAvePromotionCollection Promotions
        {
            get
            {
                if (mPromotions == null)
                {
                    PromotionCollection promotions = mKeyword.Promotions;
                    if (promotions != null)
                    {
                        mPromotions = new AvePromotionCollection(promotions);
                    }
                }
                return mPromotions;
            }
        }

        public IAveSynonymCollection Synonyms
        {
            get
            {
                if (mSynonyms == null)
                {

                    SynonymCollection synonyms = mKeyword.Synonyms;
                    if (synonyms != null)
                    {
                        mSynonyms = new AveSynonymCollection(synonyms);
                    }
                }
                return mSynonyms;
            }
        }

        public string Term
        {
            get
            {
                return mKeyword.Term;
            }
            set
            {
                mKeyword.Term = value;
            }
        }

        public IAveBestBet AddBestBet(string bestBetName)
        {
            BestBet bestBet = mKeyword.AddBestBet(bestBetName);
            if (bestBet != null)
            {
                return new AveBestBet(bestBet);
            }
            return null;
        }

        public IAveSynonym AddSynonym(string synonymTerm)
        {
            Synonym synonym = mKeyword.AddSynonym(synonymTerm);
            if (synonym != null)
            {
                return new AveSynonym(synonym);
            }
            return null;
        }

        public IAveSynonym AddSynonym(string synonymTerm, AveSynonymExpansionType type)
        {
            Synonym synonym = mKeyword.AddSynonym(synonymTerm, (SynonymExpansionType)type);
            if (synonym != null)
            {
                return new AveSynonym(synonym);
            }
            return null;
        }

        public int CompareTo(IAveKeyword other)
        {
            if (other == null || !(other is IAveKeyword))
            {
                return 1;
            }
            return mKeyword.CompareTo((other as AveKeyword).Keyword);
        }

        public long Id
        {
            get
            {
                return mKeyword.Id;
            }
        }

        public DateTime LastChanged
        {
            get
            {
                return mKeyword.LastChanged;
            }
            set
            {
                mKeyword.LastChanged = value;
            }
        }

        public bool RemoveBestBet(string bestBetName)
        {
            return mKeyword.RemoveBestBet(bestBetName);
        }

        public bool RemoveFeaturedContent(string featuredContentName)
        {
            return mKeyword.RemoveFeaturedContent(featuredContentName);
        }

        public bool RemovePromotion(string promotionName)
        {
            return mKeyword.RemovePromotion(promotionName);
        }

        public bool RemoveSynonym(string synonymTerm)
        {
            return mKeyword.RemoveSynonym(synonymTerm);
        }
    }
}
