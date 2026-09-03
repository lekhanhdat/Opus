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
using System.Collections.Generic;
using System;
using AvePoint.Wrapper.Common.Search;
using AvePoint.ObjectModel.Server16.Search;
using System.Collections.ObjectModel;

namespace AvePoint.ObjectModel.Server16
{
    class AveSearchSettingGroup : IAveSearchSettingGroup
    {
        private SearchSettingGroup mSearchSettingGroup;
        private AvePromotionCollection mPromotionsWithoutKeyword;
        private Collection<IAveBestBet> mBestBets;
        private AveBestBetCollection mBestBetsWithoutKeyword;
        private AveContextCollection mContexts;
        private Collection<IAveFeaturedContent> mFeaturedContent;
        private Collection<IAveFeaturedContent> mFeaturedContentWithKeyword;
        private AveFeaturedContentCollection mFeaturedContentWithoutKeyword;
        private AveKeywordCollection mKeywords;
        private Collection<IAvePromotion> mPromotions;
        private Collection<IAvePromotion> mPromotionsWithKeyword;

        public AveSearchSettingGroup(SearchSettingGroup searchSettingGroup)
        {
            mSearchSettingGroup = searchSettingGroup;
        }

        internal SearchSettingGroup searchSettingGroup
        {
            get
            {
                return mSearchSettingGroup;
            }
        }

        #region

        public ICollection<IAveBestBet> GetBestBets(string filter)
        {
            ICollection<BestBet> bestBets = mSearchSettingGroup.GetBestBets(filter);
            if (bestBets == null)
            {
                return null;
            }
            Collection<IAveBestBet> aveBestBets = new Collection<IAveBestBet>();
            foreach (BestBet bestBet in bestBets)
            {
                if (bestBet != null)
                {
                    aveBestBets.Add(new AveBestBet(bestBet));
                }
                else
                {
                    aveBestBets.Add(null);
                }
            }
            return aveBestBets;
        }

        public ICollection<IAveBestBet> GetBestBetsWithKeyword(string filter)
        {
            ICollection<BestBet> bestBets = mSearchSettingGroup.GetBestBetsWithKeyword(filter);
            if (bestBets == null)
            {
                return null;
            }
            Collection<IAveBestBet> aveBestBets = new Collection<IAveBestBet>();
            foreach (BestBet bestBet in bestBets)
            {
                if (bestBet != null)
                {
                    aveBestBets.Add(new AveBestBet(bestBet));
                }
                else
                {
                    aveBestBets.Add(null);
                }
            }
            return aveBestBets;
        }

        public IAveBestBetCollection GetBestBetsWithoutKeyword(string filter)
        {
            BestBetCollection bestBets = mSearchSettingGroup.GetBestBetsWithoutKeyword(filter);
            if (bestBets == null)
            {
                return null;
            }
            return new AveBestBetCollection(bestBets);
        }

        public IAveContextCollection GetContexts(string filter)
        {
            ContextCollection contexts = mSearchSettingGroup.GetContexts(filter);
            if (contexts == null)
            {
                return null;
            }
            return new AveContextCollection(contexts);
        }

        public ICollection<IAveFeaturedContent> GetFeaturedContent(string filter)
        {
            ICollection<FeaturedContent> featuredContents = mSearchSettingGroup.GetFeaturedContent(filter);
            if (featuredContents == null)
            {
                return null;
            }
            Collection<IAveFeaturedContent> aveFeaturedContent = new Collection<IAveFeaturedContent>();
            foreach (FeaturedContent featuredContent in featuredContents)
            {
                if (featuredContent != null)
                {
                    aveFeaturedContent.Add(new AveFeaturedContent(featuredContent));
                }
                else
                {
                    aveFeaturedContent.Add(null);
                }
            }
            return aveFeaturedContent;
        }

        public ICollection<IAveFeaturedContent> GetFeaturedContentWithKeyword(string filter)
        {
            ICollection<FeaturedContent> featuredContents = mSearchSettingGroup.GetFeaturedContentWithKeyword(filter);
            if (featuredContents == null)
            {
                return null;
            }
            Collection<IAveFeaturedContent> aveFeaturedContent = new Collection<IAveFeaturedContent>();
            foreach (FeaturedContent featuredContent in featuredContents)
            {
                if (featuredContent != null)
                {
                    aveFeaturedContent.Add(new AveFeaturedContent(featuredContent));
                }
                else
                {
                    aveFeaturedContent.Add(null);
                }
            }
            return aveFeaturedContent;
        }

        public IAveFeaturedContentCollection GetFeaturedContentWithoutKeyword(string filter)
        {
            FeaturedContentCollection featuredContents = mSearchSettingGroup.GetFeaturedContentWithoutKeyword(filter);
            if (featuredContents == null)
            {
                return null;
            }
            return new AveFeaturedContentCollection(featuredContents);
        }

        public IAveKeywordCollection GetKeywords(string filter)
        {
            KeywordCollection keyWords = mSearchSettingGroup.GetKeywords(filter);
            if (keyWords == null)
            {
                return null;
            }
            return new AveKeywordCollection(keyWords);
        }

        public ICollection<IAvePromotion> GetPromotions(string filter)
        {
            ICollection<Promotion> promotions = mSearchSettingGroup.GetPromotions(filter);
            if (promotions == null)
            {
                return null;
            }
            Collection<IAvePromotion> avePromotion = new Collection<IAvePromotion>();
            foreach (Promotion promotion in Promotions)
            {
                if (promotion != null)
                {
                    avePromotion.Add(new AvePromotion(promotion));
                }
                else
                {
                    avePromotion.Add(null);
                }
            }
            return avePromotion;
        }

        public ICollection<IAvePromotion> GetPromotionsWithKeyword(string filter)
        {
            ICollection<Promotion> promotions = mSearchSettingGroup.GetPromotionsWithKeyword(filter);
            if (promotions == null)
            {
                return null;
            }
            Collection<IAvePromotion> avePromotion = new Collection<IAvePromotion>();
            foreach (Promotion promotion in Promotions)
            {
                if (promotion != null)
                {
                    avePromotion.Add(new AvePromotion(promotion));
                }
                else
                {
                    avePromotion.Add(null);
                }
            }
            return avePromotion;
        }

        public IAvePromotionCollection GetPromotionsWithoutKeyword(string filter)
        {
            PromotionCollection promotions = mSearchSettingGroup.GetPromotionsWithoutKeyword(filter);
            if (promotions == null)
            {
                return null;
            }
            return new AvePromotionCollection(promotions);
        }

        public ICollection<IAveBestBet> BestBets
        {
            get
            {
                if (mBestBets == null)
                {
                    ICollection<BestBet> bestBets = mSearchSettingGroup.BestBets;
                    if (bestBets != null)
                    {
                        mBestBets = new Collection<IAveBestBet>();
                        foreach (BestBet bestBet in bestBets)
                        {
                            if (bestBet != null)
                            {
                                mBestBets.Add(new AveBestBet(bestBet));
                            }
                            else
                            {
                                mBestBets.Add(null);
                            }
                        }
                    }
                }
                return mBestBets;
            }
        }

        public ICollection<IAveBestBet> BestBetsWithKeyword
        {
            get
            {
                if (mBestBets == null)
                {
                    ICollection<BestBet> bestBets = mSearchSettingGroup.BestBetsWithKeyword;
                    if (bestBets != null)
                    {
                        mBestBets = new Collection<IAveBestBet>();
                        foreach (BestBet bestBet in bestBets)
                        {
                            if (bestBet != null)
                            {
                                mBestBets.Add(new AveBestBet(bestBet));
                            }
                            else
                            {
                                mBestBets.Add(null);
                            }
                        }
                    }
                }
                return mBestBets;
            }
        }

        public IAveBestBetCollection BestBetsWithoutKeyword
        {
            get
            {
                if (mBestBetsWithoutKeyword == null)
                {
                    BestBetCollection bestBets = mSearchSettingGroup.BestBetsWithoutKeyword;
                    if (bestBets != null)
                    {
                        mBestBetsWithoutKeyword = new AveBestBetCollection(bestBets);
                    }
                }
                return mBestBetsWithoutKeyword;
            }
        }

        public IAveContextCollection Contexts
        {
            get
            {
                if (mContexts == null)
                {
                    ContextCollection contexts = mSearchSettingGroup.Contexts;
                    if (contexts != null)
                    {
                        mContexts = new AveContextCollection(contexts);
                    }
                }
                return mContexts;
            }
        }

        public ICollection<IAveFeaturedContent> FeaturedContent
        {
            get
            {
                if (mFeaturedContent == null)
                {
                    ICollection<FeaturedContent> featuredContents = mSearchSettingGroup.FeaturedContent;
                    if (featuredContents != null)
                    {
                        mFeaturedContent = new Collection<IAveFeaturedContent>();
                        foreach (FeaturedContent featuredContent in featuredContents)
                        {
                            if (featuredContent != null)
                            {
                                mFeaturedContent.Add(new AveFeaturedContent(featuredContent));
                            }
                            else
                            {
                                mFeaturedContent.Add(null);
                            }
                        }
                    }
                }
                return mFeaturedContent;
            }
        }

        public ICollection<IAveFeaturedContent> FeaturedContentWithKeyword
        {
            get
            {
                if (mFeaturedContentWithKeyword == null)
                {
                    ICollection<FeaturedContent> featuredContents = mSearchSettingGroup.FeaturedContentWithKeyword;
                    if (featuredContents != null)
                    {
                        mFeaturedContentWithKeyword = new Collection<IAveFeaturedContent>();
                        foreach (FeaturedContent featuredContent in featuredContents)
                        {
                            if (featuredContent != null)
                            {
                                mFeaturedContentWithKeyword.Add(new AveFeaturedContent(featuredContent));
                            }
                            else
                            {
                                mFeaturedContentWithKeyword.Add(null);
                            }
                        }
                    }
                }
                return mFeaturedContentWithKeyword;
            }
        }

        public IAveFeaturedContentCollection FeaturedContentWithoutKeyword
        {
            get
            {
                if (mFeaturedContentWithoutKeyword == null)
                {
                    FeaturedContentCollection featuredcontents = mSearchSettingGroup.FeaturedContentWithoutKeyword;
                    if (featuredcontents != null)
                    {
                        mFeaturedContentWithoutKeyword = new AveFeaturedContentCollection(featuredcontents);
                    }
                }
                return mFeaturedContentWithoutKeyword;
            }
        }

        public IAveKeywordCollection Keywords
        {
            get
            {
                if (mKeywords == null)
                {
                    KeywordCollection keywords = mSearchSettingGroup.Keywords;
                    if (keywords != null)
                    {
                        mKeywords = new AveKeywordCollection(keywords);
                    }
                }
                return mKeywords;
            }
        }

        public ICollection<IAvePromotion> Promotions
        {
            get
            {
                if (mPromotions == null)
                {
                    ICollection<Promotion> promotions = mSearchSettingGroup.Promotions;
                    if (promotions != null)
                    {
                        mPromotions = new Collection<IAvePromotion>();
                        foreach (Promotion promotion in promotions)
                        {
                            if (promotion != null)
                            {
                                mPromotions.Add(new AvePromotion(promotion));
                            }
                            else
                            {
                                mPromotions.Add(null);
                            }
                        }
                    }
                }
                return mPromotions;
            }
        }

        public ICollection<IAvePromotion> PromotionsWithKeyword
        {
            get
            {
                if (mPromotionsWithKeyword == null)
                {
                    ICollection<Promotion> promotions = mSearchSettingGroup.PromotionsWithKeyword;
                    if (promotions != null)
                    {
                        mPromotionsWithKeyword = new Collection<IAvePromotion>();
                        foreach (Promotion promotion in promotions)
                        {
                            if (promotion != null)
                            {
                                mPromotionsWithKeyword.Add(new AvePromotion(promotion));
                            }
                            else
                            {
                                mPromotionsWithKeyword.Add(null);
                            }
                        }
                    }
                }
                return mPromotionsWithKeyword;
            }
        }

        public IAvePromotionCollection PromotionsWithoutKeyword
        {
            get
            {
                if (mPromotionsWithoutKeyword == null)
                {
                    PromotionCollection promotions = mSearchSettingGroup.PromotionsWithoutKeyword;
                    if (promotions != null)
                    {
                        mPromotionsWithoutKeyword = new AvePromotionCollection(promotions);
                    }
                }
                return mPromotionsWithoutKeyword;
            }
        }

        public int CompareTo(IAveSearchSettingGroup other)
        {
            if (other == null || !(other is IAveSearchSettingGroup))
            {
                return 1;
            }
            return mSearchSettingGroup.CompareTo((other as AveSearchSettingGroup).searchSettingGroup);
        }

        public string Description
        {
            get
            {
                return mSearchSettingGroup.Description;
            }
            set
            {
                mSearchSettingGroup.Description = value;
            }
        }

        public string Name
        {
            get
            {
                return mSearchSettingGroup.Name;
            }
            set
            {
                mSearchSettingGroup.Name = value;
            }
        }

        public long Id
        {
            get { return mSearchSettingGroup.Id; }
        }

        public DateTime LastChanged
        {
            get
            {
                return mSearchSettingGroup.LastChanged;
            }
            set
            {
                mSearchSettingGroup.LastChanged = value;
            }
        }

        #endregion

    }
}
